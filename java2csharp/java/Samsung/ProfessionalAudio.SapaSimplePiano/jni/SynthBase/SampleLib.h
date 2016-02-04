#ifndef SAMPLE_LIB_H
#define SAMPLE_LIB_H

#include <sys/select.h>
#include "SynthType.h"
#include "SoundFontParser.h"
#include "Synthbase_List.h"
#include "adsr.h"

#define INVALID_LOOP (-274359) // magic code

typedef enum
{
	ROM = 1,
	RAM = 2,
	SF_FONTLIST = 4,
} SampleLibStorge;

// // indicate no loop, endless loop
typedef enum
{
	NO_LOOP = 0,
	ENDLESS_LOOP,
} SampleLibLoopPlayback;

class SampleLibInfo{
	public:
		SFSampleLink type;
		SampleLibLoopPlayback loopPlay;
		int root;  //MIDI root note
		int low;  //low note
		int high;  //high note
		int pos;  // start position
		int end;  // sample end
		int loop; // loop start

		float outr, outl;
		float releaseDec;
		ADSR_PRAM adsr_parm;

		float samplerate;
		void *nextlink;
		short *samplePCM;

        SampleLibInfo(int root = 0, int low = 0 ) {
            this->root = root;
			this->low = low;

			/* InstrumentSampleInfo.gen[...] */
			this->high = 0;  //high note
			this->loop = INVALID_LOOP;
			this->loopPlay = NO_LOOP;
			this->outr = 1.0f;
			this->outl = 1.0f;

			pos = 0;
			end = 0;
			releaseDec = 0;
			samplerate = 48000;
			nextlink = NULL;
			samplePCM = NULL;
			type = monoSample;
        }

        bool operator<(const SampleLibInfo& a) const {
            return low < a.low;
        }

		bool operator>=(const SampleLibInfo& a) const {
            return low >= a.low;
        }
};

class fontSetting {
public:
	fd_set flag;
	enum {
		FS_ATTENUATION,
		FS_PAN,
		FS_ATTACK,
		FS_DECAY,
		FS_SUSTAIN,
		FS_RELEASE,
		FS_END
	};
	int attenuation;
	int pan;
	int attack;
	int decay;
	int sustain;
	int release;

	fontSetting() {
		FD_ZERO(&flag);
	}

	int sfGenListDig(sfGenList *plist);
	int flagIsSet( int val ){
		return FD_ISSET(val, &flag);
		}
	void flagSet(int val) {
		FD_SET(val,&flag);
	}

};

class SampleLib
{
public:
	SampleLib( SampleLibStorge ,int, SampleLibInfo  *);
	SampleLib( const char *fontname, int Bank, int Presets );
	~SampleLib(void);

	bool sampleLibInfo( int32  note, SampleLibInfo &Info, int &startindex );
	int numOfSamples;
protected:
	SampleLibStorge sampleStorge;
	QLIST<SampleLibInfo> info;
	QLIST<SampleLibInfo>::Node *it;
};
#endif
