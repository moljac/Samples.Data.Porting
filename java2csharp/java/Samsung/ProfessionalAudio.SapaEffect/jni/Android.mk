LOG_TAG := audiosuite:sapaeffectsample:n

LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE    := libsapaclient
LOCAL_SRC_FILES := apa/lib/libsapaclient.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libjack
LOCAL_SRC_FILES := apa/lib/libjack.so
include $(PREBUILT_SHARED_LIBRARY)

include $(CLEAR_VARS)

MY_PACKAGE_NAME := com.samsung.audiosuite.sapaeffectsample

LOCAL_CPP_EXTENSION := .cpp
LOCAL_MULTILIB := 32
LOCAL_MODULE    := wave
LOCAL_SRC_FILES := \
	EffectSampleAPAClient.cpp \
	EffectSampleJackClient.cpp

LOCAL_C_INCLUDES := $(LOCAL_PATH) \
                    $(LOCAL_PATH)/apa/include

MY_OPTIM_FLAGS += -march=armv7-a -mfloat-abi=softfp -mfpu=neon
ifneq ($(APP_OPTIM),debug)
MY_OPTIM_FLAGS += -O3 -ffast-math
endif
LOCAL_CFLAGS += $(MY_OPTIM_FLAGS)
LOCAL_CXXFLAGS += $(CXXFLAGS) $(MY_OPTIM_FLAGS)
LOCAL_CXXFLAGS += -std=c++11 -Wall -Wextra
LOCAL_CPPFLAGS += $(CPPFLAGS)
LOCAL_CPPFLAGS += -DLOG_TAG=\"$(LOG_TAG)\"
LOCAL_CPPFLAGS += -DPACKAGE_NAME=\"$(MY_PACKAGE_NAME)\"
LOCAL_CPPFLAGS += -DPACKAGE_DIR=\"/data/data/$(MY_PACKAGE_NAME)\"

LOCAL_CPP_FEATURES := exceptions rtti
LOCAL_LDLIBS := -llog

LOCAL_SHARED_LIBRARIES := libjack
LOCAL_STATIC_LIBRARIES := libsapaclient
LOCAL_ARM_MODE := arm
include $(BUILD_SHARED_LIBRARY)
