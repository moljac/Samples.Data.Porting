#pragma once
/**
 * @date        Jun 09, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include "jack_types.h"

#include <string>


namespace audiosuite
{

class StereoPort
{
    jack_port_t* left{nullptr};
    jack_port_t* right{nullptr};

public:
    StereoPort(jack_client_t* jc, const std::string& name, bool output);

    /* retrievs buffer of the left output port */
    sample_t* getLeftPortBuffer(jack_nframes_t nframes) noexcept;

    /* retrievs buffer of the right output port */
    sample_t* getRightPortBuffer(jack_nframes_t nframes) noexcept;
};

} /* namespace audiosuite */
