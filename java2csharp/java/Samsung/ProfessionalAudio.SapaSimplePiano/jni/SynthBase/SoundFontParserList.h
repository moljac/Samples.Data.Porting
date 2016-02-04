#ifndef __SOUND_FONT_PARSER_LIST_H__
#define __SOUND_FONT_PARSER_LIST_H__

#include <stdio.h>

template <typename T>
struct node{
	T data;
	node* next;
};

////////////////////////////////////////////////////////////////////

template <typename T>
class SoundFontParserList {

public:
	SoundFontParserList();
	~SoundFontParserList();

	T* insert(T &item);
	T* begin();
	T* next();
	T* getItem(int pos);
	void deleteAllItem();
	int getItemCount();

private:
	node<T> *head;
	node<T> *tail;
	node<T> *current;
	int itemCount;
};

/////////////////////////////////////////////////////////////////////

template <typename T>
SoundFontParserList<T>::SoundFontParserList(){ 
	head = NULL; 
	tail = NULL;
	current = NULL;
	itemCount = 0;
}

template <typename T>
SoundFontParserList<T>::~SoundFontParserList(){
	deleteAllItem();
}

template <typename T>
T* SoundFontParserList<T>::insert(T &item){

	if(NULL == head){
		head = (node<T>*) new node<T> ;
		memset(head, 0, sizeof(node<T>));
		head->data = item;
		current = head;
		tail = head;
	}
	else{
		tail->next = (node<T>*) new node<T>;
		memset(tail->next, 0, sizeof(node<T>));

		tail->next->data = item;
		tail = tail->next;
	}
	
	tail->next = NULL;
	itemCount++;
	return &tail->data;
}

template <typename T>
T* SoundFontParserList<T>::begin(){

	if (NULL == head){
		return NULL;
	}
	current = head;
	return &head->data;
}

template <typename T>
T* SoundFontParserList<T>::next(){

	if (NULL == current->next){
		return NULL;
	}

	current = current->next;
	return &current->data;
}

template <typename T>
void SoundFontParserList<T>::deleteAllItem(){
	// free all node
	node<T> *pos = head;
	node<T> *posNext = NULL;
	while (NULL != pos){
		posNext = pos->next;
		delete pos;
		if (NULL != posNext){
			pos = posNext;
		}
		else{
			pos = NULL;
		}
	}
	itemCount = 0;
}

template <typename T>
T* SoundFontParserList<T>::getItem(int pos){
	if (NULL == head){
		current = 0;
		return NULL;
	}
	node<T> *ret = head;
	for (int i = 0; i < pos; i++){
		ret = ret->next;
		if (NULL == ret){
			return NULL;
		}
	}
	current = ret;
	return &ret->data;
}

template <typename T>
int SoundFontParserList<T>::getItemCount(){
	return itemCount;
}
#endif