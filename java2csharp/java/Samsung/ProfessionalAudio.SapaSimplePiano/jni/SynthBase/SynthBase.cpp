#include "SynthBase.h"
#include "SynthBase_math.h"

SynthBase::SynthBase(float rate = SAMPLE_RATE, const char *fontname = "SM1.sf2", int Bank = 0, int Presets = 1 ):samplib( fontname, Bank, Presets),lfo(rate)
{
	initialize();
}

SynthBase::SynthBase(float rate, SampleLibStorge storage ,int n , SampleLibInfo  *info ):samplib( storage, n, info),lfo(rate)
{
	// Todo for RAM and ROM Font support
	initialize();
}

SynthBase::~SynthBase(void)
{
	// ???
}

void SynthBase::initialize (void)
{
	lfo.setRate(10);
	lfo.setSampleRate(SAMPLE_RATE);
	lfo.setWaveform(LFO::sinus);

	//initialise...
	for(int32 v=0; v<NVOICES; v++) 
	{
		it = new(struct QLIST<VOICE>::Node);
		it->value.env=0.0f;
		it->value.decCount=0xFFFFFFFF;

		freeVoices.InsertNodefirst(it);
	}

	sustain = 0;
	activevoices = 0;
	velsens = 1.0f;
	poly = NVOICES - 1;//8 + (int32)(24.9f * Polyphony);
	eventPos = 0;
	eventPools[0] = EVENTS_DONE;
	pitch_wheel = PITCH_RANGE_CENTER;
	modulation_wheel = 0;

}

void SynthBase::pushEvents ( int32 offset,int32  pitch, int32 velocity )
{
	if (eventPos > EVENTBUFFER) eventPos -= 3; //discard events if buffer full!!

	eventPools[eventPos++] = offset;
	eventPools[eventPos++] = pitch;
	eventPools[eventPos++] = velocity; // * 127;

	eventPools[eventPos] = EVENTS_DONE;
}

void SynthBase::pushControl ( int32 offset,int32  control, int32 value )
{
	if (eventPos > EVENTBUFFER) eventPos -= 3; //discard events if buffer full!!

	eventPools[eventPos++] = offset;
	eventPools[eventPos++] = -control;
	eventPools[eventPos++] = value; // * 127;

	eventPools[eventPos] = EVENTS_DONE;
}

void SynthBase::noteOn(int note, int velocity)
{

	SampleLibInfo Info;
	int searchIndex = 0;

	if (velocity>0) 
	{
		do{
			if (samplib.sampleLibInfo( note, Info, searchIndex ) )
				StartVoice( Info, note, velocity );
		}while( samplib.numOfSamples > searchIndex );
	}
	else
	{
		StopVoice( note );
	}
}

void SynthBase::StartVoice( SampleLibInfo &info, int note, int velocity )
{
	float l= 99.0f;
	int32 rootpitch;
	unsigned long decCount = 0xFFFFFFFF;
	VOICE *pV;
	struct QLIST<VOICE>::Node *temp;
	float x;

	if (activevoices < poly) //add a note
	{
		if (activevoices > (poly - ((NVOICES-1)*0.2)) ){

			it = activeVoices.FirstNode();
			do{
				if ((it->value.decCount < decCount) && ((it->value.adsr_proc.getState() != (int)ADSR_PROC::env_fadeout)||(it->value.adsr_proc.getState() != (int)ADSR_PROC::env_remove))) {
					decCount = it->value.decCount;
				}
				it=it->next;
			}while(it != NULL);
			it = activeVoices.FirstNode();
			do{
				if (it->value.decCount == decCount){
					it->value.adsr_proc.SetStatEnvFadeout();
				}
				it=it->next;
			}while(it != NULL); 
		}
		it = freeVoices.FirstNode();
		freeVoices.transferFront(activeVoices, it );
		pV = &(it->value);
		activevoices++;
	}
	else //steal a note
	{
		// TODO: force fade out & mix feature
		temp = it = activeVoices.FirstNode();
		do{
			if (it->value.decCount < decCount){
				decCount = it->value.decCount;
				temp = it;
			}
			it=it->next;
		}while(it != NULL);
		
		//printf("note off:%i note:%i", vl, note);
#if  1// note skip and // activevoices 숫자가 많아지면 시작도 못하게 되니  fade out 시켜두었으니 stop시 소리가 끊어지는 현상은 적을 듯
		it = activeVoices.FirstNode();
		do{
			if (it->value.decCount == decCount){
				it->value.adsr_proc.SetStatEnvRemove();
			}
			it=it->next;
		}while(it != NULL); 

		return;
#else
		pV = &(temp->value);
#endif
	}

	// voice initialize


	pV->state = Active;
	pV->note = note;
	rootpitch = info.root;
	pV->pos = info.pos;
	pV->end = info.end;
	pV->loop = info.loop;
	pV->waves = info.samplePCM;
	pV->decCount = 0xFFFFFFFF;

	pV->frac = 0;

	pV->adsr_proc.setAdsrParm(info.adsr_parm);
	pV->adsr_proc.gate(1);


	// TODO:range check EXP_T[]
	l = (float)EXP_T[NOTE_RANGE_POG + (note - rootpitch)];
	l = info.samplerate *l;
	pV->delta = (int32)(65536.0f * l);

	//velocity calculator
	x = (float)POW_T[velocity - 1][int(velsens * 10)];
	pV->outr = info.outr * x;
	pV->outl = info.outl * x;

}

void SynthBase::StopVoice( int note ) 
{
	it = activeVoices.FirstNode();
	if(it == NULL ) 
		return;

	do{
		if (sustain==0){
			if (it->value.note==note){
					it->value.adsr_proc.gate(0);
					it->value.state = DeActive;
				}
		}
		else{
			if (it->value.note==note){
					it->value.state = DeActive;
				}
		}
		
		it=it->next;
	}while(it != NULL);
}

void SynthBase::processing( float *channelBuffers32, int32 numSamples )
{
	int32 sampleFrames = numSamples;
		
	float* out0 = &channelBuffers32[0];
	float* out1 = &channelBuffers32[numSamples];

	int32 event=0, frame=0, frames;
	float x, l, r;
	int32 i;
	VOICE *pV;
	struct QLIST<VOICE>::Node *temp;

	while (frame<sampleFrames)
	{
		frames = eventPools[event++]; 
		if (frames>sampleFrames) frames = sampleFrames;
		frames -= frame;
		frame += frames;

		while (--frames>=0)
		{
			l = r = 0.0f;

			for(it = activeVoices.FirstNode();it != NULL;it=it->next)
			{
				pV = &(it->value);
				if( pitch_wheel != PITCH_RANGE_CENTER )
					pV->frac += (int32)(pV->delta * (PITCH_WHEEL_T[pitch_wheel]));
				else
					pV->frac += pV->delta;
				pV->pos += pV->frac >> 16;
				pV->frac &= 0xFFFF;
				pV->decCount--;

				if (pV->pos >= pV->end){
					if(pV->loop != INVALID_LOOP){
						pV->pos = pV->loop;
					}else{
						pV->env = 0.0f;
						pV->pos = pV->end;
					}
				}

				i = pV->waves[pV->pos] + ((pV->frac * (pV->waves[pV->pos + 1] - pV->waves[pV->pos])) >> 16);

				// signed short in to float out
				((unsigned &)x)=0x43818000^((unsigned short)i);
				x-=259.0f;

				x *= pV->env; // envelope calculator
				pV->env = pV->adsr_proc.process();

				l += pV->outl * x;
				r += pV->outr * x;
			}

			// insert effector here... Only for each of voice
			if(modulation_wheel){
				x = lfo.tick();
				l = l*0.923880f + l*0.382683f * x;
				r = r*0.923880f + r*0.382683f * x;
			}
			*out0++ = l;
			*out1++ = r;
		}

		if (frame<sampleFrames)
		{
			int32 ctrl = eventPools[event++];
			int32 param  = eventPools[event++];
			if( ctrl >= 0 ){
				noteOn(ctrl, param);
			}
			else{
				ctrl =- ctrl;
				switch( ctrl )
				{
					case cc_PITCH_WHEEL:
						pitch_wheel = param;
						break;
					case cc_MODULATION:
						modulation_wheel = param;
						lfo.setRate((float)param*0.15f);
						break;
					case cc_SUSTAIN:
						sustain = param;
						if (sustain == 0) {
							for (it = activeVoices.FirstNode(); it != NULL; it = it->next) {
								if (it->value.state == DeActive){
									it->value.adsr_proc.gate(0);
								}
							}
						}
 						break;
					case cc_ALL_NOTES_OFF:
						for(it = activeVoices.FirstNode();it != NULL;it=it->next) 
							it->value.adsr_proc.SetStatEnvFadeout();
						break;
					case cc_ALL_NOTES_SOUND_OFF:
						for (it = activeVoices.FirstNode(); it != NULL; it = it->next)
							it->value.adsr_proc.SetStatEnvRemove();
						break;
					default:
						break;
				}
			}
		}
	}
	it = activeVoices.FirstNode();
	while(it != NULL)
	{
		if (it->value.env < SILENCE){
			//printf("SILENCE: state:%i ",it->value.state);
			temp = it;
			it=it->next;
			temp->value.state = Off;
			activeVoices.transferFront( freeVoices, temp); // move to free pool list
			activevoices--;
			//printf("sustain:%i eventPos:%i activevoice:%i \n",sustain, eventPos, activevoices);
		}
		else
			it=it->next;
	}
	eventPools[0] = EVENTS_DONE; //mark events buffer as done
	eventPos = 0;
}

