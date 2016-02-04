
#include "SoundFontParser.h"
#include "../mylog.h"
#include <stdio.h>
#include <string.h>
#include <stdlib.h>

SoundFontParser::SoundFontParser(){
	fp = NULL;
	initParser(NULL);
}

SoundFontParser::~SoundFontParser(){
	// free allocated Info-list memory
	if (mInfo.comments){
		free(mInfo.comments);
		mInfo.comments = NULL;
	}
	if (NULL != fp){
		fclose(fp);
	}
}

SoundFontParserResult SoundFontParser::parsing(char* sf2FilePath){

	SoundFontParserResult result = fail;

	if ((result = initParser(sf2FilePath)) < 0){
		LOGE("SoundFontParser::parsing initParser() failed path[%s], result[%d]", sf2FilePath, result);
		return result;
	}

	// check that is this sound font format file
	if ((result = isSoundFontFile()) < 0) {
		LOGE("SoundFontParser::parsing isSoundFontFile() failed path[%s], result[%d]", sf2FilePath, result);
		return result;
	}
	// parse <INFO-list> chunk
	if ((result = parseINFO()) < 0) {
		LOGE("SoundFontParser::parsing parseINFO() failed path[%s], result[%d]", sf2FilePath, result);
		return result;
	}
	// parse <sdta-list> chunk
	if ((result = parseSDTA()) < 0) {
		LOGE("SoundFontParser::parsing parseSDTA() failed path[%s], result[%d]", sf2FilePath, result);
		return result;
	}
	// parse <pdta-list> chunk
	if ((result = parsePDTA()) < 0) {
		LOGE("SoundFontParser::parsing parsePDTA() failed path[%s], result[%d]", sf2FilePath, result);
		return result;
	}
	return success;
}

int SoundFontParser::getSampleCount(){

	return mPDTA.shdr.getItemCount();
}

SoundFontParserResult SoundFontParser::getPresetInfo(int bank, int preset, PresetInfo &infoClass){

	infoClass.bank = bank;
	infoClass.preset = preset;
	sfPresetHeader *phdr = mPDTA.phdr.begin();

	// find correct phdr chunk
	phdr = mPDTA.phdr.begin();
	while (NULL != phdr){
		if (phdr->wBank == bank && phdr->wPreset == preset){
			break;
		}
		phdr = mPDTA.phdr.next();
		if (NULL == phdr){
			return no_bank_preset;
		}
	}

	// find pbag start, end point
	int pbagStart = phdr->wPresetBagNdx;
	int pbagEnd = 0;
	sfPresetHeader *nextPhdr = mPDTA.phdr.next();
	if (NULL == nextPhdr){
		pbagEnd = mPDTA.pbag.getItemCount();
	}
	else{
		pbagEnd = nextPhdr->wPresetBagNdx;
	}

	for (int i = pbagStart; i < pbagEnd; i++){
		sfPresetBag *pbag = mPDTA.pbag.getItem(i);

		// find pgen start, end point
		int pgenStart = pbag->wGenNdx;
		int pgenEnd = 0;

		sfPresetBag *nextPgenNext = mPDTA.pbag.next();
		if (NULL == nextPgenNext){
			pgenEnd = mPDTA.pgen.getItemCount();
		}
		else {
			pgenEnd = nextPgenNext->wGenNdx;
		}
		
		if (pgenEnd - pgenStart <= 0){
			continue;
		}

		// check global zone
		int isGlobal = 0;
		if ((i == pbagStart) && (mPDTA.pgen.getItem(pgenEnd - 1)->sfGenOper != SPA_instrument)){
			isGlobal = 1;
		}

		if (isGlobal){
			for (int j = pgenStart; j < pgenEnd; j++){
				infoClass.globalGenCount = 0;
				infoClass.globalGen[infoClass.globalGenCount].sfGenOper = mPDTA.pgen.getItem(j)->sfGenOper;
				infoClass.globalGen[infoClass.globalGenCount].genAmount = mPDTA.pgen.getItem(j)->genAmount;
				infoClass.globalGenCount++;
			}
		}
		else{
			// if last item is not instrument then skip
			if (mPDTA.pgen.getItem(pgenEnd - 1)->sfGenOper != SPA_instrument){
				continue;
			}

			PresetInstrumentInfo instInfo;
			instInfo.genCount = 0;
			for (int j = pgenStart; j < pgenEnd; j++){
				instInfo.gen[instInfo.genCount].sfGenOper = mPDTA.pgen.getItem(j)->sfGenOper;
				instInfo.gen[instInfo.genCount].genAmount = mPDTA.pgen.getItem(j)->genAmount;

				instInfo.genCount++;

				if (SPA_instrument == mPDTA.pgen.getItem(j)->sfGenOper){
					instInfo.instrumentID = (int)mPDTA.pgen.getItem(j)->genAmount.shAmount;
					memcpy(instInfo.name, mPDTA.inst.getItem(instInfo.instrumentID)->achInstName, INSTRUMENT_NAME_LEN);
					break;
				}
			}
			infoClass.instrumentInfo.insert(instInfo);
		}
	}
	return success;
}

SoundFontParserResult SoundFontParser::getInstumentInfo(int instrument, InstrumentInfo &infoClass){

	// check that instrument param is valid
	if (instrument < 0 || instrument >= mPDTA.inst.getItemCount()){
		return no_instrument;
	}
	
	infoClass.instrumentID = instrument;
	sfInst *inst = mPDTA.inst.getItem(instrument);
	memcpy(infoClass.name, inst->achInstName, INSTRUMENT_NAME_LEN);

	// ibag start, end
	int ibagStart = inst->wInstBagNdx;
	int ibagEnd = 0;

	sfInst *instNext = mPDTA.inst.next();
	if (NULL == instNext){
		ibagEnd = mPDTA.ibag.getItemCount();
	}
	else{
		ibagEnd = instNext->wInstBagNdx;
	}

	for (int i = ibagStart; i < ibagEnd; i++){

		sfInstBag *ibag = mPDTA.ibag.getItem(i);

		// find igen start/end 
		int igenStart = ibag->wInstGenNdx;
		int igenEnd = 0;
		sfInstBag *ibagNext = mPDTA.ibag.next();

		if (NULL == ibagNext){
			igenEnd = mPDTA.igen.getItemCount();
		}
		else{
			igenEnd = ibagNext->wInstGenNdx;
		}

		if (igenEnd - igenStart <= 0){
			continue;
		}

		// check global zone
		int isGlobal = 0;
		if ((i == ibagStart) && (mPDTA.igen.getItem(igenEnd - 1)->sfGenOper != SPA_sampleID)){
			isGlobal = 1;
		}

		if (isGlobal){
			infoClass.globalGenCount = 0;
			for (int j = igenStart; j < igenEnd; j++){
				sfInstGenList *igen = mPDTA.igen.getItem(j);
				infoClass.globalGen[infoClass.globalGenCount].sfGenOper = igen->sfGenOper;
				infoClass.globalGen[infoClass.globalGenCount].genAmount = igen->genAmount;
				infoClass.globalGenCount++;
			}
		}
		else{
			// if last item is not sampleID then skip
			if (mPDTA.igen.getItem(igenEnd - 1)->sfGenOper != SPA_sampleID){
				continue;
			}

			InstrumentSampleInfo sampleInfo;
			sampleInfo.genCount = 0;
			for (int j = igenStart; j < igenEnd; j++){
				sfInstGenList *igen = mPDTA.igen.getItem(j);
				sampleInfo.gen[sampleInfo.genCount].sfGenOper = igen->sfGenOper;
				sampleInfo.gen[sampleInfo.genCount].genAmount = igen->genAmount;
				sampleInfo.genCount++;

				if (SPA_sampleID == igen->sfGenOper){
					memcpy(sampleInfo.sampleName, mPDTA.shdr.getItem(igen->genAmount.shAmount)->achSampleName , SAMPLE_NAME_LEN);
					sampleInfo.sampleID = igen->genAmount.shAmount;
					break;
				}
			}
			infoClass.sampleInfo.insert(sampleInfo);
		}
	}

	return success;
}

SoundFontParserResult SoundFontParser::getSampleInfo(int sampleID, SampleInfo &infoClass){

	sfSample *shdr = mPDTA.shdr.getItem(sampleID);

	if (NULL == shdr){
		return no_sample;
	}

	memcpy(infoClass.name, shdr->achSampleName, SAMPLE_NAME_LEN);

	int sampleSize = (shdr->dwEnd - shdr->dwStart) * 2;
	infoClass.sampleSize = sampleSize;
	if (NULL != infoClass.sampleData){
		free(infoClass.sampleData);
	}
	infoClass.sampleData = malloc(sampleSize);
	sampleSize = readPCM(mSDTA.smplStartPosition + shdr->dwStart * 2, sampleSize, infoClass.sampleData);

	infoClass.dwStartloop = (shdr->dwStartloop - shdr->dwStart) * 2;
	infoClass.dwEndloop = (shdr->dwEndloop - shdr->dwStart) * 2;
	infoClass.dwSampleRate = shdr->dwSampleRate;
	infoClass.byOriginalPitch = shdr->byOriginalPitch;
	infoClass.chPitchCorrection = shdr->chPitchCorrection;
	infoClass.wSampleLink = shdr->wSampleLink;
	infoClass.sfSampleType = shdr->sfSampleType;

	return success;
}


/////////////////////////////////////////////////////////////////////////////////////////////
SoundFontParserResult SoundFontParser::initParser(char* sf2FilePath){

	if (NULL != fp){
		fclose(fp);
		fp = NULL;
	}

	if (NULL != sf2FilePath){
		if (NULL == (fp = fopen(sf2FilePath, "rb"))){
		    LOGE("SoundFontParser::initParser file open failed [%s]", sf2FilePath);
			return file_open_failed;
		}
	}
	mFileOffset = 0;

	memset(FOURCC, 0, 5);
	mRIFFTotalSize = 0;

	// init info struct
	memset(&mInfo, 0, sizeof(SF2INFO));

	// init sdta struct
	memset(&mSDTA, 0, sizeof(SF2SDTA));

	// init pdta struct
	mPDTA.phdr.deleteAllItem();
	mPDTA.pbag.deleteAllItem();
	mPDTA.pmod.deleteAllItem();
	mPDTA.pgen.deleteAllItem();
	mPDTA.inst.deleteAllItem();
	mPDTA.ibag.deleteAllItem();
	mPDTA.imod.deleteAllItem();
	mPDTA.igen.deleteAllItem();
	mPDTA.shdr.deleteAllItem();

	return success;
}

SoundFontParserResult SoundFontParser::isSoundFontFile(){

	char* readedRIFF = readFOURCC();

	if(NULL == readedRIFF ){
		return no_sound_font_file;
	}

	if (strncmp(readedRIFF, "RIFF", 4)){
		return no_sound_font_file;
	}
	mRIFFTotalSize = readDWORD() + 8;

	char* readedSFBK = readFOURCC();

	if(NULL == readedSFBK ){
    	return no_sound_font_file;
    }

	if (strncmp(readedSFBK, "sfbk", 4)){
		return no_sound_font_file;
	}
	return success;
}

// parse <INFO-list> chunk
SoundFontParserResult SoundFontParser::parseINFO(){

	if (strncmp(readFOURCC(), "LIST", 4)){
		return no_info_list;
	}
	DWORD infoListSize = readDWORD();
	long int nextChunkPosition = mFileOffset + infoListSize;

	if (strncmp(readFOURCC(), "INFO", 4)){
		return no_info_list;
	}

	while (mFileOffset < nextChunkPosition - 1) {
		char * chunkID = readFOURCC();

		if (!strncmp("ifil", chunkID, 4)){
			if (4 != readDWORD()) {
				return no_info_list;
			}
			mInfo.soundFontVersionMajor = readWORD();
			mInfo.soundFontVersionMinor = readWORD();
		}
		else if (!strncmp("isng", chunkID, 4)){
			readString(mInfo.soundEngine, readDWORD());
		}
		else if (!strncmp("INAM", chunkID, 4)){
			readString(mInfo.bankName, readDWORD());
		}
		else if (!strncmp("irom", chunkID, 4)){
			readString(mInfo.romName, readDWORD());
		}
		else if (!strncmp("iver", chunkID, 4)){
			if (4 != readDWORD()) {
				break;  // this size 4 byte only
			}
			mInfo.romRevisionMajor = readWORD();
			mInfo.romRevisionMinor = readWORD();
		}
		else if (!strncmp("ICRD", chunkID, 4)){
			readString(mInfo.creationDate, readDWORD());
		}
		else if (!strncmp("IENG", chunkID, 4)){
			readString(mInfo.designerName, readDWORD());
		}
		else if (!strncmp("IPRD", chunkID, 4)){
			readString(mInfo.product, readDWORD());
		}
		else if (!strncmp("ICOP", chunkID, 4)){
			readString(mInfo.copyRight, readDWORD());
		}
		else if (!strncmp("ICMT", chunkID, 4)){
			//ICMT's size is big so it need malloc/free to get data.
			if (mInfo.comments){
				free(mInfo.comments);
				mInfo.comments = NULL;
			}
			long cmtSize = readDWORD();
			mInfo.comments = (char*)malloc(cmtSize);
			readString(mInfo.comments, cmtSize);
		}
		else if (!strncmp("ISFT", chunkID, 4)){
			readString(mInfo.creationTool, readDWORD());
		}
		else{
			break;
		}
	}

	// set to next chunk, It may be <sdta-list> chunk
	mFileOffset = nextChunkPosition;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return file_seek_failed;
	};
	return success;
}

SoundFontParserResult SoundFontParser::parseSDTA(){

	if (strncmp(readFOURCC(), "LIST", 4)){
		return no_sdta_list;
	}
	DWORD infoListSize = readDWORD();
	long int nextChunkPosition = mFileOffset + infoListSize;

	// sdta is mandatory
	if (strncmp(readFOURCC(), "sdta", 4)){
		return no_sdta_list;
	}

	while (mFileOffset < nextChunkPosition - 1) {
		char * chunkID = readFOURCC();
		if (!strncmp("smpl", chunkID, 4)){
			mSDTA.smplSize = readDWORD();
			mSDTA.smplStartPosition = mFileOffset;
			mFileOffset += mSDTA.smplSize;
			if (fseek(fp, mFileOffset, SEEK_SET)){
				return file_seek_failed;
			};
		}
		else if (!strncmp("sm24", chunkID, 4)){
			mSDTA.sm24Size = readDWORD();
			mSDTA.sm24StartPosition = mFileOffset;
			mFileOffset += mSDTA.sm24Size;
			if (fseek(fp, mFileOffset, SEEK_SET)){
				return file_seek_failed;
			};
		}
		else{
			break;
		}
	}

	// set to next chunk, It may be <pdta-list> chunk
	mFileOffset = nextChunkPosition;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return file_seek_failed;
	};
	return success;
}

SoundFontParserResult SoundFontParser::parsePDTA(){

	if (strncmp(readFOURCC(), "LIST", 4)){
		return no_pdta_list;
	}
	DWORD infoListSize = readDWORD();
	long int nextChunkPosition = mFileOffset + infoListSize;

	if (strncmp(readFOURCC(), "pdta", 4)){
		return no_pdta_list;
	}
	
	while (mFileOffset < nextChunkPosition - 1) {
		char* chunkID = readFOURCC();
		DWORD size = readDWORD();
	    long int nextChunkPosition = mFileOffset + size;

		if (!strncmp("PHDR", chunkID, 4) || !strncmp("phdr", chunkID, 4)){
			parsePDTA_PHDR(nextChunkPosition);
		}
		else if (!strncmp("PBAG", chunkID, 4) || !strncmp("pbag", chunkID, 4)){
			parsePDTA_PBAG(nextChunkPosition);
		}
		else if (!strncmp("PMOD", chunkID, 4) || !strncmp("pmod", chunkID, 4)){
			parsePDTA_PMOD(nextChunkPosition);
		}
		else if (!strncmp("PGEN", chunkID, 4) || !strncmp("pgen", chunkID, 4)){
			parsePDTA_PGEN(nextChunkPosition);
		}
		else if (!strncmp("INST", chunkID, 4) || !strncmp("inst", chunkID, 4)){
			parsePDTA_INST(nextChunkPosition);
		}
		else if (!strncmp("IBAG", chunkID, 4) || !strncmp("ibag", chunkID, 4)){
			parsePDTA_IBAG(nextChunkPosition);
		}
		else if (!strncmp("IMOD", chunkID, 4) || !strncmp("imod", chunkID, 4)){
			parsePDTA_IMOD(nextChunkPosition);
		}
		else if (!strncmp("IGEN", chunkID, 4) || !strncmp("igen", chunkID, 4)){
			parsePDTA_IGEN(nextChunkPosition);
		}
		else if (!strncmp("SHDR", chunkID, 4) || !strncmp("shdr", chunkID, 4)){
			parsePDTA_SHDR(nextChunkPosition);
		}
		else{
			break;
		}

		mFileOffset = nextChunkPosition;
		if (fseek(fp, mFileOffset, SEEK_SET)){  // go to next chunk
			return file_seek_failed;
		}; 
	}
	
	// go to next chunk, It may be EOF
	mFileOffset = nextChunkPosition;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return file_seek_failed;
	};
	return success;
}

SoundFontParserResult SoundFontParser::parsePDTA_PHDR(long int nextChunkPos){

	while (mFileOffset < nextChunkPos - 1)
	{
		sfPresetHeader data;
		readString(data.achPresetName, 20);
		data.wPreset = readWORD();
		data.wBank = readWORD();
		data.wPresetBagNdx = readWORD();
		data.dwLibrary = readDWORD();
		data.dwGenre = readDWORD();
		data.dwMorphology = readDWORD();
		mPDTA.phdr.insert(data);

		// check EOP
		char isEOP[4] = { 0, };
		int readedSize = fread(isEOP, sizeof(char), 3, fp);
		if (readedSize <= 0){
			return file_read_failed;
		}
			
		if (!strncmp("EOP", isEOP, 3)){
			break;
		}
		else{
			if( fseek(fp, mFileOffset, SEEK_SET) ){
				return file_seek_failed;
			};
		}
	}
	return success;
}

SoundFontParserResult SoundFontParser::parsePDTA_PBAG(long int nextChunkPos){

	while (mFileOffset < nextChunkPos - 1){
		sfPresetBag data;
		data.wGenNdx = readWORD();
		data.wModNdx = readWORD();
		mPDTA.pbag.insert(data);
	}
	return success;
}

SoundFontParserResult SoundFontParser::parsePDTA_PMOD(long int nextChunkPos){
	
	while (mFileOffset < nextChunkPos - 1){
		sfModList data;
		data.sfModSrcOper = readSFModulator();
		data.sfModDestOper = readSFGenerator();
		data.modAmount = readSHORT();
		data.sfModAmtSrcOper = readSFModulator();
		data.sfModTransOper = readSFTransform();
		mPDTA.pmod.insert(data);
	}
	return success;
}

SoundFontParserResult SoundFontParser::parsePDTA_PGEN(long int nextChunkPos){

	while (mFileOffset < nextChunkPos - 1){
		sfGenList data;
		data.sfGenOper = (SF2GeneratorEnumerators)readSFGenerator();
		data.genAmount = readGenAmountType();
		mPDTA.pgen.insert(data);
	}
	return success;
}

SoundFontParserResult SoundFontParser::parsePDTA_INST(long int nextChunkPos){

	while (mFileOffset < nextChunkPos - 1){
		sfInst data;
		readString(data.achInstName, 20);
		data.wInstBagNdx = readWORD();
		mPDTA.inst.insert(data);

		// check EOI
		char isEOI[4] = { 0, };
		int readedSize = fread(isEOI, sizeof(char), 3, fp);
		if ( readedSize <= 0){
			return file_read_failed;
		};

		if (!strncmp("EOI", isEOI, 3)){
			break;
		}
		else{
			if( fseek(fp, mFileOffset, SEEK_SET)){
				return file_seek_failed;
			};
		}
	}
	return success;
}

SoundFontParserResult SoundFontParser::parsePDTA_IBAG(long int nextChunkPos){

	while (mFileOffset < nextChunkPos - 1){
		sfInstBag data;
		data.wInstGenNdx = readWORD();
		data.wInstModNdx = readWORD();
		mPDTA.ibag.insert(data);
	}
	return success;
}

SoundFontParserResult SoundFontParser::parsePDTA_IMOD(long int nextChunkPos){

	while (mFileOffset < nextChunkPos - 1){
		sfModList data;
		data.sfModSrcOper = readSFModulator();
		data.sfModDestOper = readSFGenerator();
		data.modAmount = readSHORT();
		data.sfModAmtSrcOper = readSFModulator();
		data.sfModTransOper = readSFTransform();
		mPDTA.imod.insert(data);
	}
	return success;
}

SoundFontParserResult SoundFontParser::parsePDTA_IGEN(long int nextChunkPos){

	while (mFileOffset < nextChunkPos - 1){
		sfInstGenList data;
		data.sfGenOper = (SF2GeneratorEnumerators) readSFGenerator();
		data.genAmount = readGenAmountType();
		mPDTA.igen.insert(data);
	}
	return success;
}

SoundFontParserResult SoundFontParser::parsePDTA_SHDR(long int nextChunkPos){
	
	while (mFileOffset < nextChunkPos - 1){
		sfSample data;
		readString(data.achSampleName, 20);
		data.dwStart = readDWORD();
		data.dwEnd = readDWORD();
		data.dwStartloop = readDWORD();
		data.dwEndloop = readDWORD();
		data.dwSampleRate = readDWORD();
		data.byOriginalPitch = readBYTE();
		data.chPitchCorrection = readCHAR();
		data.wSampleLink = readWORD();
		data.sfSampleType = (SFSampleLink) readWORD();
		mPDTA.shdr.insert(data);

		// check EOS
		char isEOS[4] = { 0, };
		int readedSize = fread(isEOS, sizeof(char), 3, fp);
		if ( readedSize <= 0){
			return file_read_failed;
		};

		if (!strncmp("EOS", isEOS, 3)){
			break;
		}
		else{
			if (fseek(fp, mFileOffset, SEEK_SET)){
				return file_seek_failed;
			};
		}
	}
	return success;
}

///////////////// Read File Functions /////////////////
char* SoundFontParser::readFOURCC(){
	
	memset(FOURCC, 0, 5);
	int readedSize = fread(FOURCC, sizeof(char), 4, fp);
	if ( readedSize <= 0){
		return NULL;
	};
	mFileOffset += 4;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return NULL;
	};
	return FOURCC;
}

DWORD SoundFontParser::readDWORD(){

	DWORD result = 0;
	int readedSize = fread(&result, sizeof(DWORD), 1, fp);
	if (readedSize <= 0){
		return result;
	};
	mFileOffset += 4;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return result;
	};
	return result;
}

WORD SoundFontParser::readWORD(){

	WORD result = 0;
	int readedSize = fread(&result, sizeof(WORD), 1, fp);
	if ( readedSize <= 0){
		return result;
	};
	mFileOffset += 2;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return result;
	};
	return result;
}

SHORT SoundFontParser::readSHORT(){
	SHORT result = 0;
	int readedSize = fread(&result, sizeof(SHORT), 1, fp);
	if ( readedSize <= 0){
		return result;
	};
	mFileOffset += 2;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return result;
	};
	return result;
}

BYTE SoundFontParser::readBYTE(){
	BYTE result = 0;
	int readedSize = fread(&result, sizeof(BYTE), 1, fp);
	if ( readedSize <= 0){
		return result;
	};
	mFileOffset += 1;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return result;
	};
	return result;
}

CHAR SoundFontParser::readCHAR(){
	CHAR result = 0;
	int readedSize = fread(&result, sizeof(CHAR), 1, fp);
	if ( readedSize <= 0){
		return result;
	};
	mFileOffset += 1;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return result;
	};
	return result;
}

void SoundFontParser::readString(char* buf, DWORD size){
	int readedSize = fread(buf, sizeof(char), size, fp);
	if ( readedSize <= 0){
		return;
	};
	mFileOffset += size;

	if (fseek(fp, mFileOffset, SEEK_SET)){
		return;
	};
}

SFGenerator SoundFontParser::readSFGenerator(){
	SFGenerator result = 0;
	int readedSize = fread(&result, sizeof(SFGenerator), 1, fp);
	if ( readedSize <= 0){
		return result;
	};
	mFileOffset += 2;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return result;
	};
	return result;
}

SFModulator SoundFontParser::readSFModulator(){
	SFModulator result = 0;
	int readedSize = fread(&result, sizeof(SFModulator), 1, fp);
	if ( readedSize <= 0){
		return result;
	};
	mFileOffset += 2;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return result;
	};
	return result;
}

SFTransform SoundFontParser::readSFTransform(){
	SFTransform result = 0;
	int readedSize = fread(&result, sizeof(SFTransform), 1, fp);
	if ( readedSize <= 0){
		return result;
	};
	mFileOffset += 2;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return result;
	};
	return result;
}

genAmountType SoundFontParser::readGenAmountType(){
	genAmountType result;
	int readedSize = fread(&result, sizeof(genAmountType), 1, fp);
	if (readedSize <= 0){
		return result;
	};
	mFileOffset += 2;
	if (fseek(fp, mFileOffset, SEEK_SET)){
		return result;
	};
	return result;
}

long int SoundFontParser::readPCM(long int start, long int size, void *buf){
	int ret = 0;
	if (fseek(fp, start, SEEK_SET)){
		return 0;
	};
	ret = fread((char *)buf, sizeof(char), size, fp);
	return ret;
}

PresetInstrumentInfo::PresetInstrumentInfo(){
	memset(name, 0, INSTRUMENT_NAME_LEN);
	instrumentID = 0;
	genCount = 0;
	for (int i = 0; i < GENERATOR_LEN; i++){
		gen[i].sfGenOper = SPA_EOE;
		gen[i].genAmount.shAmount = 0;
	}
}

PresetInfo::PresetInfo(){
	bank = 0;
	preset = 0;
	globalGenCount = 0;
	for (int i = 0; i < GENERATOR_LEN; i++){
		globalGen[i].sfGenOper = SPA_EOE;
		globalGen[i].genAmount.shAmount = 0;
	}
}

InstrumentSampleInfo::InstrumentSampleInfo(){
	memset(sampleName, 0, SAMPLE_NAME_LEN);
	sampleID = 0;
	genCount = 0;
	for (int i = 0; i < GENERATOR_LEN; i++){
		gen[i].sfGenOper = SPA_EOE;
		gen[i].genAmount.shAmount = 0;
	}
}

InstrumentInfo::InstrumentInfo(){
	memset(name, 0, INSTRUMENT_NAME_LEN);
	instrumentID = 0;
	globalGenCount = 0;
	for (int i = 0; i < GENERATOR_LEN; i++){
		globalGen[i].sfGenOper = SPA_EOE;
		globalGen[i].genAmount.shAmount = 0;
	}
}

SampleInfo::SampleInfo(){
	memset(name, 0, SAMPLE_NAME_LEN);
	sampleID = 0;
	sampleSize = 0;  
	sampleData = NULL;
	dwStartloop = 0;
	dwEndloop = 0;
	dwSampleRate = 0;
	byOriginalPitch = 0;
	chPitchCorrection = 0;
	wSampleLink = 0;
	sfSampleType = monoSample;
}

SampleInfo::~SampleInfo(){
	if (NULL != sampleData){
		free(sampleData);
	}
}
