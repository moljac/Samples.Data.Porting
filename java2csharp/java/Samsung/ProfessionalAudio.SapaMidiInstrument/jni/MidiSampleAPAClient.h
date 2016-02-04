#pragma once

/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include <IAPAInterface.h>

#include <memory>


namespace audiosuite
{
namespace sapamidisample
{

class MidiSampleJackClient;
class MidiSampleAPAClient : public ::android::IAPAInterface
{
    std::unique_ptr<MidiSampleJackClient> jackClient;

public:
    MidiSampleAPAClient();
    virtual ~MidiSampleAPAClient();

    virtual int init();
    virtual android::IJackClientInterface* getJackClientInterface();
    virtual int request(const char*, const long, const long, size_t&,
            void* const);
    /**
     * Command callback, when command send from the application layer
     * @see SapaProcessor
     * @return result code
     */
    virtual int sendCommand(const char*);
};

} /* namespace sapamidisample */
} /* namespace audiosuite */


DECLARE_APA_INTERFACE(
        ::audiosuite::sapamidisample::MidiSampleAPAClient
        )
