#pragma once

/**
 * @date        Aug 20, 2013
 * @copyright   Copyright (c) 2013 by Samsung Electronics Polska Sp. z o. o.
 */

#include <cstdint>

#pragma GCC system_header
#include <jack/types.h>


typedef jack_default_audio_sample_t sample_t;

#define JACK_TRUE 1
#define JACK_FALSE 0

#define JACK_RETURN_SUCCESS 0
#define JACK_RETURN_ERROR 1
