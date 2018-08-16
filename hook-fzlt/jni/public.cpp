#include <map>
#include <string.h>
#include <string>
#include <pthread.h>
#include <signal.h>
#include "public.h"
#include "Hook.h"


JNIEXPORT jint JNICALL JNI_OnLoad(JavaVM* vm, void* reserved)
{
    jint result = JNI_ERR;
    jint version = 0;
    JNIEnv* env = 0;
	
    do
    {
        if(vm->GetEnv((void**)&env, JNI_VERSION_1_6) == JNI_OK)
        {
        	version = JNI_VERSION_1_6;
        }
        else if(vm->GetEnv((void**)&env, JNI_VERSION_1_4) == JNI_OK)
        {
        	version = JNI_VERSION_1_4;
        }
        else if(vm->GetEnv((void**)&env, JNI_VERSION_1_2) == JNI_OK)
        {
        	version = JNI_VERSION_1_2;
        }
        else if(vm->GetEnv((void**)&env, JNI_VERSION_1_1) == JNI_OK)
        {
        	version = JNI_VERSION_1_1;
        }
        else
        {
        	
        	break;
        }
    	result = version;

    } while(0);


    return result;
}




void *thread_hook(void *arg)  
{
	LOGD("==============new thread xx:%d==============",gettid());
	sleep(1);
	while(true){

		if(hook_game_proxy()){
			break;
		}
		sleep(1);
	}
	

	return ((void *)0);
}

__attribute__((constructor)) void entry()
{

	LOGI("=======================Enter lib entry=====================");
	
	
	int temp;
	pthread_t ntid; 
	if((temp=pthread_create(&ntid,NULL,thread_hook,NULL)))  
	{  
		LOGE("can't create thread: %s\n",strerror(temp));  
	}

}
