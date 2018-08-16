LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)

LOCAL_MODULE := myhook


LOCAL_C_INCLUDES := \
					$(LOCAL_PATH)/ \
					$(LOCAL_PATH)/libzip/
					
LOCAL_CFLAGS := -DANDROID_NDK -Wno-psabi \
				-DDISABLE_IMPORTGL 
				

LOCAL_SRC_FILES +=\
			public.cpp\
			Hook.cpp

LOCAL_LDLIBS :=  -ldl -llog -lz

LOCAL_ARM_MODE := arm 

include $(BUILD_SHARED_LIBRARY)