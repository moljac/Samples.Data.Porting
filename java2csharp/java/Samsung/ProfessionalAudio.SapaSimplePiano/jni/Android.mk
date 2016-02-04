# Copyright (C) 2009 The Android Open Source Project
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
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
LOCAL_MODULE := libsynthbase
LOCAL_C_INCLUDES := $(LOCAL_PATH)/apa/include
LOCAL_SRC_FILES :=	SynthBase/SampleLib.cpp \
					SynthBase/SynthBase.cpp \
					SynthBase/SynthBase_math.cpp \
					SynthBase/lfo.cpp \
					SynthBase/SoundFontParser.cpp \
					SynthBase/adsr.cpp

LOCAL_ARM_MODE := arm
LOCAL_CXXFLAGS += -std=c++11
LOCAL_SHARED_LIBRARIES := libjack
include $(BUILD_STATIC_LIBRARY)


include $(CLEAR_VARS)
LOCAL_MODULE    := wave
LOCAL_C_INCLUDES := $(LOCAL_PATH)/apa/include
LOCAL_SRC_FILES := \
	wave.cpp \
	JackSimpleClient.cpp

LOCAL_CXXFLAGS += -std=c++11
LOCAL_CPP_FEATURES := exceptions
LOCAL_MODULE_TAGS := eng optional
LOCAL_LDLIBS := -llog
LOCAL_SHARED_LIBRARIES := libjack
LOCAL_STATIC_LIBRARIES := libsapaclient libsynthbase
LOCAL_ARM_MODE := arm
include $(BUILD_SHARED_LIBRARY)
