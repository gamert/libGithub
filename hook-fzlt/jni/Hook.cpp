#include "Hook.h"
#include "Log.h"
#include <unistd.h>
#include <stdio.h>
#include <dlfcn.h>
#include <string.h>
#include <sys/time.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <string.h>


struct _MonoImage {  
    int   ref_count;  
    void *raw_data_handle;  
    char *raw_data;  
    int raw_data_len;  
};


void printAddr(char *addr)
{
	LOGI("Addr %p : %02x %02x %02x %02x %02x %02x %02x %02x",addr,addr[0],addr[1],addr[2],addr[3],addr[4],addr[5],addr[6],addr[7]);
}


void* get_module_base(const char* module_name)
{
	FILE *fp;
	long addr = 0;
	char *pch;
	char filename[32];
	char line[1024];

	snprintf(filename, sizeof(filename), "/proc/self/maps");

	fp = fopen(filename, "r");

	if (fp != NULL) {
		while (fgets(line, sizeof(line), fp)) {
			if (strstr(line, module_name)) {
				pch = strtok(line, "-");
				addr = strtoul(pch, NULL, 16);
				if (addr == 0x8000)
					addr = 0;
				break;
			}
		}
		fclose(fp);
	}
	return (void *)addr;
}

//ARM inline hook
typedef int (*FP_GAME_PROXY)(char *data, size_t data_len, int a3, void *a4, char a5, char* name);
FP_GAME_PROXY old_game_proxy = 0;

char g_HookCode_game_proxy[8] = { 0 }; 
char g_OrigCode_game_proxy[16] = { 0 }; 
//HOOK currentFunc 跳到 targetFunc
void inlineHook_game_proxy(void* currentFunc, void* targetFunc)
{
	LOGI("inlineHook_game_proxy currentFunc-->%p targetFunc-->%p",currentFunc,targetFunc);

	int offset = (int)targetFunc;
	//ldr pc,[pc, #-4] 通过操作pc寄存器实现跳转
	g_HookCode_game_proxy[0] = 0x04;
	g_HookCode_game_proxy[1] = 0xf0;
	g_HookCode_game_proxy[2] = 0x1f;
	g_HookCode_game_proxy[3] = 0xe5;

	//跳转地址
	g_HookCode_game_proxy[4] = offset&0xff;
	g_HookCode_game_proxy[5] = (offset>>8)&0xff;
	g_HookCode_game_proxy[6] = (offset>>16)&0xff;
	g_HookCode_game_proxy[7] = (offset>>24)&0xff;

	//保存原函数头
	int saveNum = 8; //需要保存的函数头的字节数
	char *tmp = (char*)currentFunc;
	for(int i=0;i<saveNum;i++){
		g_OrigCode_game_proxy[i] = tmp[i];
	}
	

	//函数头设置属性可写
	void* page_start = (void*)((long)tmp - (long)tmp % PAGE_SIZE);
	if(-1 == mprotect((void *)page_start, PAGE_SIZE, PROT_READ | PROT_WRITE | PROT_EXEC)) {
		LOGE("mprotect failed(%d)", errno );
		return;
	}
	//替换函数头,把跳转指令写进去
	for(int i=0;i<8;i++){
		tmp[i] = g_HookCode_game_proxy[i];
	}

	offset = (int)currentFunc+8;
	//ldr pc,[pc, #-4] 通过操作pc寄存器实现跳转
	g_OrigCode_game_proxy[8] = 0x04;
	g_OrigCode_game_proxy[9] = 0xf0;
	g_OrigCode_game_proxy[10] = 0x1f;
	g_OrigCode_game_proxy[11] = 0xe5;
	//跳回原函数地址+8的位置
	g_OrigCode_game_proxy[12] = offset&0xff;
	g_OrigCode_game_proxy[13] = (offset>>8)&0xff;
	g_OrigCode_game_proxy[14] = (offset>>16)&0xff;
	g_OrigCode_game_proxy[15] = (offset>>24)&0xff;

	//buf设置可执行权限，等待被调用
	page_start = (void*)((long)g_OrigCode_game_proxy - (long)g_OrigCode_game_proxy % PAGE_SIZE);
	if(-1 == mprotect((void *)page_start, PAGE_SIZE, PROT_READ | PROT_WRITE | PROT_EXEC)) {
		LOGE("mprotect failed(%d)", errno );
		return;
	}
	old_game_proxy = (FP_GAME_PROXY)(g_OrigCode_game_proxy);

	LOGI("=======inlineHook_game_proxy finished========");
}


int new_game_proxy(char *data, size_t data_len, int a3, void *a4, char a5, char* name)
{
	int retValue = 0;
	LOGD("Hook: new_game_proxy %s==================",name);
	
	struct _MonoImage* result;  
	retValue = old_game_proxy(data,data_len,a3,a4,a5,name);

	if (strstr(name,"Assembly-CSharp.dll"))
	{
		char *pDll = "/data/local/tmp/Assembly-CSharp.dll";
		if (access(pDll, 0) == -1)
		{
			//Assembly-CSharp.dll不存在则dump
			FILE *file = fopen(pDll, "wb+");
			if (file != NULL) {
				fwrite (result->raw_data , 1, result->raw_data_len, file );
				fclose(file);
				LOGI("dump file %s successfully!!");
			}

		}else{
			//存在则加载，并替换原来的dll
			FILE *fpDll;  
			fpDll = fopen(pDll, "r");  
			if(fpDll != NULL)  
			{  
				fseek(fpDll, 0, SEEK_END);  
				int len = ftell(fpDll);  
				fseek(fpDll, 0, SEEK_SET);  
				char *mono_data = (char *)malloc(len);  
				fread(mono_data, 1, len, fpDll);  
				fclose(fpDll);  
				result->raw_data = mono_data;
				result->raw_data_len = len;
				LOGD("===============replace Assembly-CSharp.dll==================");
			}  
		}
	}
	return retValue;	
}
bool hook_game_proxy()
{
	LOGD("Hook:====hook_game_proxy begin====");

	bool state = false;
	do 
	{
		void *mono_func = get_module_base("libmono.so");

		if (mono_func == NULL)
		{
			LOGE("Hook:find mono_func failed!\n");
			break;
		}
		mono_func += 0x196C4C; //mono_image_open_from_data_with_name在libmono.so中的偏移

		//hook之前打印函数头8个字节
		LOGI("=============original code====================");
		printAddr((char*)mono_func);

		inlineHook_game_proxy(mono_func,(void*)new_game_proxy);

		//hook之后打印函数头8个字节
		LOGI("=============modified code====================");
		printAddr((char*)mono_func);

		state = true;


	} while (false);

	LOGD("Hook:=====hook_game_proxy finish=====");
	return state;
} 