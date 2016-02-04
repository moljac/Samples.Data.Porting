#ifndef SYNTHBASE_LIST
#define SYNTHBASE_LIST

#include <stdlib.h>

template <typename T>
class QLIST{
public:
	struct Node
	{
	    T value;
	    struct Node* prev;
		struct Node* next;
	};
private:
	Node *head;
	int count;
public:

QLIST(void){
	head=(Node *)malloc(sizeof(Node));
	head->prev=NULL;
	head->next=NULL;
	count = 0;
}

~QLIST(){
	while (DeleteNode(head->next)) {;}
	free(head);
	head=NULL;
}

Node *InsertNodeRight(Node *Target,Node *aNode)
{
	Node *New;
	Node *Right;

	New=aNode;

	Right=Target->next;
	New->next=Right;
	New->prev=Target;
	Target->next=New;

	if (Right) {
		Right->prev=New;
	}
	count++;
    return New;
}

Node *InsertNodeLeft(Node *Target,Node *aNode)
{
     Node *Left;

	 Left=Target->prev;
     if (Left) {
          return InsertNodeRight(Left,aNode);
     }
     return NULL;
}

Node *InsertNodefirst(Node *aNode)
{
     return InsertNodeRight(head,aNode);
}

void AppendNode(Node *aNode)
{
     Node *tail;

     for (tail=head;tail->next;tail=tail->next) {;}
	InsertNodeRight(tail,aNode);
}

int RemoveNode(Node *Target)
{
     Node *Left,*Right;

     if (Target==NULL || Target==head) {
          return false;
     }
     Left=Target->prev;
     Right=Target->next;

     Left->next=Right;
     if (Right){
          Right->prev=Left;
     }
	 count--;

    return true;
}

int DeleteNode(Node *Target){
	if(RemoveNode( Target )){
		free(Target);
		return true;
	}
	return false;
}

void transferFront(QLIST &x, Node *it){
	RemoveNode(it);
	x.InsertNodefirst(it);
}

void transferBack(QLIST &x, Node *it){
	RemoveNode(it);
	x.AppendNode(it);
}

Node *FirstNode(void)
{
     return head->next;
}

int listCount(void)
{
     return count;
}

Node *FindNodeByIndex(int idx)
{
     Node *Now;
     int Index=0;

     for (Now=head->next;Now;Now=Now->next) {
          if (Index == idx) {
              return Now;
          }
          Index++;
     }
     return NULL;
}

int GetNodeIndex(Node *Target)
{
     Node *Now;
     int Index=0;

     for (Now=head->next;Now;Now=Now->next) {
          if (Now == Target) {
              return Index;
          }
          Index++;
     }
     return -1;
}

#if 0
int GetListCount()
{
     Node *Now;
     int Count=0;

     for (Now=head->next;Now;Now=Now->next) {
          Count++;
     }
     return Count;
}
#endif
};

#endif