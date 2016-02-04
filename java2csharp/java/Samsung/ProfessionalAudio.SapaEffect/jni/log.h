#pragma once

#include <android/log.h>


#ifdef NDEBUG

#define LOGA(expr, fmt, ...) static_cast<void>(0)
#define SLOGA(expr, text) static_cast<void>(0)

#define LOGT(fmt, ...) static_cast<void>(0)
#define LOGD(fmt, ...) static_cast<void>(0)

#define SLOGT(text) static_cast<void>(0)
#define SLOGD(text) static_cast<void>(0)

#else

#define LOGA(expr, fmt, ...)                                                 \
    ((expr)                                                                  \
     ? static_cast<void>(0)                                                  \
     : __android_log_assert(__STRING(expr), LOG_TAG, "%s:%d  %s:  " fmt,     \
         __FILE__, __LINE__, __PRETTY_FUNCTION__, __VA_ARGS__))

#define LOGT(fmt, ...) __android_log_print(ANDROID_LOG_VERBOSE, LOG_TAG,     \
        "%s:  " fmt, __PRETTY_FUNCTION__, __VA_ARGS__)
#define LOGD(fmt, ...) __android_log_print(ANDROID_LOG_DEBUG,   LOG_TAG,     \
        "%s:  " fmt, __PRETTY_FUNCTION__, __VA_ARGS__)

#define SLOGA(expr, text)                                                    \
    ((expr)                                                                  \
     ? static_cast<void>(0)                                                  \
     : __android_log_assert(__STRING(expr), LOG_TAG, "%s:%d  %s:  " text,    \
         __FILE__, __LINE__, __PRETTY_FUNCTION__))

#define SLOGT(text) __android_log_print(ANDROID_LOG_VERBOSE, LOG_TAG,        \
        "%s:  " text, __PRETTY_FUNCTION__)
#define SLOGD(text) __android_log_print(ANDROID_LOG_DEBUG,   LOG_TAG,        \
        "%s:  " text, __PRETTY_FUNCTION__)
#endif

#define LOGI(fmt, ...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, fmt,   \
        __VA_ARGS__)
#define LOGW(fmt, ...) __android_log_print(ANDROID_LOG_WARN, LOG_TAG, fmt,   \
        __VA_ARGS__)
#define LOGE(fmt, ...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, fmt,  \
        __VA_ARGS__)
#define LOGF(fmt, ...) __android_log_print(ANDROID_LOG_FATAL, LOG_TAG, fmt,  \
        __VA_ARGS__)

#define SLOGI(text) __android_log_write(ANDROID_LOG_INFO, LOG_TAG, text)
#define SLOGW(text) __android_log_write(ANDROID_LOG_WARN, LOG_TAG, text)
#define SLOGE(text) __android_log_write(ANDROID_LOG_ERROR, LOG_TAG, text)
#define SLOGF(text) __android_log_write(ANDROID_LOG_FATAL, LOG_TAG, text)
