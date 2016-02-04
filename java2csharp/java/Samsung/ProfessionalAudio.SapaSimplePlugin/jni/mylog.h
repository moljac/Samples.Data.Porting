#ifndef _MY_LOG_H_
#define _MY_LOG_H_

#include <android/log.h>

namespace android {

#define LOG_TAG "SimpleClientNative"

#define LOGD(...) __android_log_print(ANDROID_LOG_DEBUG,LOG_TAG,__VA_ARGS__)

#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR,LOG_TAG,__VA_ARGS__)

#define LOGW(...) __android_log_print(ANDROID_LOG_WARN,LOG_TAG,__VA_ARGS__)

#define LOGV(...) __android_log_print(ANDROID_LOG_VERBOSE,LOG_TAG,__VA_ARGS__)
};

#endif


