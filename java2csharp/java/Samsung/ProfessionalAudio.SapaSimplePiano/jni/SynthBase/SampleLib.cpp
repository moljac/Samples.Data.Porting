#include <math.h>
#include <cstdlib>
#include <string.h>
#include <stdlib.h>

#include "SampleLib.h"
#include "SoundFontParser.h"
#include "SynthBase_math.h"

template<typename T>
void selectSort(QLIST<T> &x, int n) {
	typename QLIST<T>::Node *itI;
	typename QLIST<T>::Node *itJ;
	typename QLIST<T>::Node *iMin;

	T temp;
	int i,j;
     
	for (itI = x.FirstNode(), i=0 ;i<n-1;i++){
         iMin = itI = x.FindNodeByIndex(i);

		 j=i+1;
		 for (  ;j<n;j++) {
			 itJ = x.FindNodeByIndex(j);
             if ( (itJ->value.low) < (iMin->value.low) ) {
                iMin = itJ;
             }
         } 
         if (iMin != itI) {
            // the smallest item is somewhere else in the array so move it to this position
            memcpy( &temp, &(itI->value), sizeof(T));
            memcpy( &(itI->value), &(iMin->value), sizeof(T));
            memcpy( &(iMin->value),&temp, sizeof(T));
         }   
     }
}

int fontSetting::sfGenListDig( sfGenList *plist )
{
	int sfGenOper = plist->sfGenOper;
	
	if(sfGenOper == SPA_initialAttenuation)
	{
		attenuation = (int32)(plist->genAmount.shAmount);
		flagSet(FS_ATTENUATION);
	}
	else if (sfGenOper == SPA_pan)
	{
		pan = (int32)(plist->genAmount.shAmount) / 10; // -500 ~ +500  pan_gain_ref() support -50 ~ +50 => /10
		flagSet(FS_PAN);
	}
	else if (sfGenOper == SPA_attackVolEnv)
	{
		attack = plist->genAmount.shAmount;
		flagSet(FS_ATTACK);
	}
	else if (sfGenOper == SPA_decayVolEnv)
	{
		decay = plist->genAmount.shAmount;	
		flagSet(FS_DECAY);
	}
	else if (sfGenOper == SPA_sustainVolEnv)
	{
		sustain = plist->genAmount.shAmount;
		flagSet(FS_SUSTAIN);
	}
	else if (sfGenOper == SPA_releaseVolEnv)
	{
		release = plist->genAmount.shAmount;	
 		flagSet(FS_RELEASE);
	}
	
	return sfGenOper;
}


SampleLib::SampleLib( SampleLibStorge storge, int n, SampleLibInfo  *sampleLibInfo )
{
	sampleStorge = storge;
	numOfSamples = n;
	it = NULL;
	//TODO .... memcpy( Info, sampleLibInfo, n*sizeof(SampleLibInfo) );
}

SampleLib::SampleLib( const char *fontname, int Bank, int Presets  )
{
	int j, val;
	float x,y;

	sampleStorge = RAM;
	numOfSamples = 0;
	it = NULL;

	SoundFontParser parser;
	SoundFontParserResult result;

	// start sf2 parsing
	if ((result = parser.parsing((char* )fontname)) != 0){
		printf("parser error [%d]", result);
	}

	// get preset info
	PresetInfo presetInfo;
	PresetInstrumentInfo* pPresetInsInfo;

	InstrumentInfo instInfo;
	InstrumentSampleInfo* pInstSampleInfo;

	fontSetting globalsetting;

	result = parser.getPresetInfo(Bank, Presets, presetInfo);

	SampleInfo sampleInfo;	

	// basic Synth own function
	SampleLibInfo *pSynthSampleLibInfo;

	ADSR_Coef adsr_coef;

	// achive preset global info
	for (j = 0; j<presetInfo.globalGenCount; j++){
		//Preset Global setting add
		globalsetting.sfGenListDig( &presetInfo.globalGen[j]);
	}

	for( pPresetInsInfo = presetInfo.instrumentInfo.begin(); NULL != pPresetInsInfo; pPresetInsInfo = presetInfo.instrumentInfo.next() )
	{
		for(j=0;j<pPresetInsInfo->genCount;j++){
			//Preset Instrument setting add
			globalsetting.sfGenListDig( &pPresetInsInfo->gen[j]);
		}

		result = parser.getInstumentInfo(pPresetInsInfo->instrumentID, instInfo);		
		for(j=0;j<instInfo.globalGenCount;j++){
			//Instrument Globa settingl add
			globalsetting.sfGenListDig( &instInfo.globalGen[j]);
		}		
		numOfSamples += instInfo.sampleInfo.getItemCount();

		for(  pInstSampleInfo = instInfo.sampleInfo.begin(); NULL != pInstSampleInfo; pInstSampleInfo = instInfo.sampleInfo.next() )
		{	
			//printf("sampleInfo name : %s\n", pInstSampleInfo->sampleName );
			result = parser.getSampleInfo( pInstSampleInfo->sampleID , sampleInfo);
			if (result  != 0){
				printf("parser error [%d]", result);
			}
			it = new(struct QLIST<SampleLibInfo>::Node);
			info.InsertNodefirst(it);
			pSynthSampleLibInfo = &(it->value);

			pSynthSampleLibInfo->root = sampleInfo.byOriginalPitch;


			if(globalsetting.flagIsSet(fontSetting::FS_ATTENUATION)){
				pSynthSampleLibInfo->outr = Attenuation_tab[globalsetting.attenuation];
				pSynthSampleLibInfo->outl = Attenuation_tab[globalsetting.attenuation];
			}else{
				pSynthSampleLibInfo->outr = 1.0;
				pSynthSampleLibInfo->outl = 1.0;
			}

			if (globalsetting.flagIsSet(fontSetting::FS_PAN)) {
				val = globalsetting.pan;
				/* pan table */
				pan_gain_ref(val, x, y);
				pSynthSampleLibInfo->outl *= x;
				pSynthSampleLibInfo->outr *= y;
			}

			if (globalsetting.flagIsSet(fontSetting::FS_ATTACK)) {
				x = (float)globalsetting.attack;
				y = (float)pow(10, (x)*(1.0f / 1200.0f)*0.301029996f); //LOG10(2) = 0.301029996 // sf release vol로 부터 시간의 값을 얻고
				adsr_coef.setAttackRate(y*SAMPLE_RATE);
			}
			if (globalsetting.flagIsSet(fontSetting::FS_DECAY)) {
				x = (float)globalsetting.decay;
				y = (float)pow(10, (x)*(1.0f / 1200.0f)*0.301029996f); //LOG10(2) = 0.301029996 // sf release vol로 부터 시간의 값을 얻고
				adsr_coef.setDecayRate(y*SAMPLE_RATE);
			}
			if (globalsetting.flagIsSet(fontSetting::FS_SUSTAIN)) {
				val = globalsetting.sustain;
				adsr_coef.setSustainLevel(Attenuation_tab[val]);
			}
			if (globalsetting.flagIsSet(fontSetting::FS_RELEASE)) {
				x = (float)globalsetting.release;
				y = (float)pow(10, (x)*(1.0f / 1200.0f)*0.301029996f); //LOG10(2) = 0.301029996 // sf release vol로 부터 시간의 값을 얻고
				adsr_coef.setReleaseRate(y*SAMPLE_RATE);
			}

			for (j = 0; j < pInstSampleInfo->genCount; j++)
			{
				if (pInstSampleInfo->gen[j].sfGenOper == SPA_keyRange)
				{
					pSynthSampleLibInfo->high = (int32)pInstSampleInfo->gen[j].genAmount.ranges.byHi;
					//printf("pSampleLibInfo.high : %i\n", pSynthSampleLibInfo->high );
					pSynthSampleLibInfo->low = (int32)pInstSampleInfo->gen[j].genAmount.ranges.byLo;
				}
				else if (pInstSampleInfo->gen[j].sfGenOper == SPA_initialAttenuation)
				{
					val = (int32)(pInstSampleInfo->gen[j].genAmount.shAmount);
					pSynthSampleLibInfo->outr *= Attenuation_tab[val];
					pSynthSampleLibInfo->outl *= Attenuation_tab[val];
				}
				else if (pInstSampleInfo->gen[j].sfGenOper == SPA_pan)
				{
					val = (int32)(pInstSampleInfo->gen[j].genAmount.shAmount) / 10; // -500 ~ +500  pan_gain_ref() support -50 ~ +50 => /10
					if (val == 0) {
						pSynthSampleLibInfo->outr *= 0.707107f; // mono voice split to R L channel and set to the half level
						pSynthSampleLibInfo->outl *= 0.707107f;
					}
					else {
						/* pan table */
						pan_gain_ref(val, x, y);
						pSynthSampleLibInfo->outl *= x;
						pSynthSampleLibInfo->outr *= y;
					}
				}
				else if (pInstSampleInfo->gen[j].sfGenOper == SPA_attackVolEnv)
				{
					x = pInstSampleInfo->gen[j].genAmount.shAmount;
					y = (float)pow(10, (x)*(1.0f / 1200.0f)*0.301029996f); //LOG10(2) = 0.301029996 // sf release vol로 부터 시간의 값을 얻고
					adsr_coef.setAttackRate(y*SAMPLE_RATE);
				}
				else if (pInstSampleInfo->gen[j].sfGenOper == SPA_decayVolEnv)
				{
					x = pInstSampleInfo->gen[j].genAmount.shAmount;
					y = (float)pow(10, (x)*(1.0f / 1200.0f)*0.301029996f); //LOG10(2) = 0.301029996 // sf release vol로 부터 시간의 값을 얻고
					adsr_coef.setDecayRate(y*SAMPLE_RATE);
				}
				else if (pInstSampleInfo->gen[j].sfGenOper == SPA_sustainVolEnv)
				{
					val = (int32)(pInstSampleInfo->gen[j].genAmount.shAmount);
					adsr_coef.setSustainLevel(Attenuation_tab[val]);
				}
				else if(pInstSampleInfo->gen[j].sfGenOper == SPA_releaseVolEnv)
				{
					x = pInstSampleInfo->gen[j].genAmount.shAmount;
					y = (float)pow(10, (x)*(1.0f / 1200.0f)*0.301029996f); //LOG10(2) = 0.301029996 // sf release vol로 부터 시간의 값을 얻고
					adsr_coef.setReleaseRate(y*SAMPLE_RATE);
				}
				else if(pInstSampleInfo->gen[j].sfGenOper == SPA_sampleModes)
				{
					if (((int32)pInstSampleInfo->gen[j].genAmount.shAmount ) == 1)
						pSynthSampleLibInfo->loopPlay = ENDLESS_LOOP;
				}
			}

			pSynthSampleLibInfo->adsr_parm = adsr_coef.getAdsrParm();
			adsr_coef.reset();
			pSynthSampleLibInfo->type = sampleInfo.sfSampleType;
			pSynthSampleLibInfo->pos = 0;

			// SPA_sampleModes indicate no loop, endless loop
			if( pSynthSampleLibInfo->loopPlay != NO_LOOP ){
				pSynthSampleLibInfo->loop = sampleInfo.dwStartloop>>1;
			}

			pSynthSampleLibInfo->end = (sampleInfo.dwEndloop == 0 ?  sampleInfo.sampleSize>>1 : sampleInfo.dwEndloop>>1 );

			pSynthSampleLibInfo->samplerate = (float)sampleInfo.dwSampleRate;
			pSynthSampleLibInfo->nextlink = NULL;
			if( sampleInfo.sampleSize > 0 )
			{
				pSynthSampleLibInfo->samplePCM = new short[sampleInfo.sampleSize>>1];
				memcpy( pSynthSampleLibInfo->samplePCM, sampleInfo.sampleData, sampleInfo.sampleSize );
			}
		} /* pInstSampleInfo */
	} /* pPresetInsInfo */
	if(info.listCount() == numOfSamples )
		selectSort( info, info.listCount() ); 
}

SampleLib::~SampleLib()
{
// ???
}

bool SampleLib::sampleLibInfo( int32  note, SampleLibInfo &Info, int &startindex )
{
	for(it = info.FindNodeByIndex(startindex);it != NULL;it=it->next)
		if( note >= (it->value.low) ){
			if( note <= ( it->value.high) ){
				memcpy( &Info, &(it->value), sizeof(SampleLibInfo) );
				startindex = info.GetNodeIndex(it)+1; //return next search index
				return 1;
			}		
		}else if(note < it->value.high){
			startindex = info.listCount() + 1; //return next search index
			return 0;
		}

	if(it == NULL)
		startindex = info.listCount() + 1; //return next search index
	else
		startindex = info.GetNodeIndex(it) + 1; //return next search index
	
	return 0;
}
