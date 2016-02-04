#ifndef SYSTH_BASE_H
#define SYSTH_BASE_H

#include "SynthType.h"
#include "Synthbase_List.h"
#include "SampleLib.h"
#include "lfo.h"
#include "adsr.h"


const float PI           = (float)3.14159265358979;
const float SILENCE		 = (float)0.0001f;  //voice choking

//#define WAVE_SHAPER

class SynthBase
{
public:
	SynthBase(void);
	SynthBase(float rate, SampleLibStorge storage ,int n , SampleLibInfo  *info );
	SynthBase(float rate, const char *fontname, int Bank, int Presets);
	~SynthBase(void);

	void pushEvents ( int32 offset,int32 note, int32 velocity );
	void pushControl ( int32 offset,int32  control, int32 value );
	void processing( float *channelBuffers32, int32 numSamples );

	enum SYNTH_D {
		NOTE_RANGE_MIN = 0,
		NOTE_RANGE_MAX = 127,
		cc_PITCH_WHEEL = 128,
		cc_MODULATION,
		cc_SUSTAIN,
		cc_ALL_NOTES_OFF,
		cc_ALL_NOTES_SOUND_OFF,
		END_SYNTH_CTRL
	};

protected:
	SampleLib samplib;
	LFO lfo;

	enum {
		EVENTBUFFER=120,
		EVENTS_DONE=99999999,
		NVOICES=62,
	};

	enum  voicestate : int16 {
	    Off =  0x0,
		Active = 0x1,
		DeActive = 0x2,
//	    STYLE =  0x40,
//	    STYLE =  0x80,
	};

	struct VOICE  //voice state
	{
		voicestate state;
		int16 note; //remember what note triggered this
		unsigned int decCount;

		int32 delta;  //sample playback
		int32 frac;

		short *waves; //sample data
		int32 pos; 
		int32 end;
		int32 loop;

		float env;  //envelope

		ADSR_PROC adsr_proc;

		float outl; //level & pan
		float outr;
	};
	struct QLIST<VOICE>::Node *it;

	void initialize (void);
	void noteOn(int note, int velocity);
	void StopVoice( int note );
	void StartVoice( SampleLibInfo &info,int note, int velocity );

	int32 eventPos;
	int32 eventPools[EVENTBUFFER + 8];  //list of delta|note|velocity for current block

	QLIST<VOICE> activeVoices;
	QLIST<VOICE> freeVoices;
	int32 activevoices, poly;
	int32 sustain;
	float velsens;
	int32 pitch_wheel;
	int32 modulation_wheel;
};
#endif
