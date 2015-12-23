using System;
using System.Collections.Generic;

namespace com.firebase.androidchat
{

	using Activity = android.app.Activity;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseAdapter = android.widget.BaseAdapter;

	using ChildEventListener = com.firebase.client.ChildEventListener;
	using DataSnapshot = com.firebase.client.DataSnapshot;
	using FirebaseError = com.firebase.client.FirebaseError;
	using Query = com.firebase.client.Query;


	/// <summary>
	/// @author greg
	/// @since 6/21/13
	/// 
	/// This class is a generic way of backing an Android ListView with a Firebase location.
	/// It handles all of the child events at the given Firebase location. It marshals received data into the given
	/// class type. Extend this class and provide an implementation of <code>populateView</code>, which will be given an
	/// instance of your list item mLayout and an instance your class that holds your data. Simply populate the view however
	/// you like and this class will handle updating the list as the data changes.
	/// </summary>
	/// @param <T> The class type to use as a model for the data contained in the children of the given Firebase location </param>
	public abstract class FirebaseListAdapter<T> : BaseAdapter
	{

		private Query mRef;
		private Type<T> mModelClass;
		private int mLayout;
		private LayoutInflater mInflater;
		private IList<T> mModels;
		private IList<string> mKeys;
		private ChildEventListener mListener;


		/// <param name="mRef">        The Firebase location to watch for data changes. Can also be a slice of a location, using some
		///                    combination of <code>limit()</code>, <code>startAt()</code>, and <code>endAt()</code>, </param>
		/// <param name="mModelClass"> Firebase will marshall the data at a location into an instance of a class that you provide </param>
		/// <param name="mLayout">     This is the mLayout used to represent a single list item. You will be responsible for populating an
		///                    instance of the corresponding view with the data from an instance of mModelClass. </param>
		/// <param name="activity">    The activity containing the ListView </param>
		public FirebaseListAdapter(Query mRef, Type<T> mModelClass, int mLayout, Activity activity)
		{
			this.mRef = mRef;
			this.mModelClass = mModelClass;
			this.mLayout = mLayout;
			mInflater = activity.LayoutInflater;
			mModels = new List<T>();
			mKeys = new List<string>();
			// Look for all child events. We will then map them to our own internal ArrayList, which backs ListView
			mListener = this.mRef.addChildEventListener(new ChildEventListenerAnonymousInnerClassHelper(this));
		}

		private class ChildEventListenerAnonymousInnerClassHelper : ChildEventListener
		{
			private readonly FirebaseListAdapter<T> outerInstance;

			public ChildEventListenerAnonymousInnerClassHelper(FirebaseListAdapter<T> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onChildAdded(DataSnapshot dataSnapshot, string previousChildName)
			{

				T model = dataSnapshot.getValue(outerInstance.mModelClass);
				string key = dataSnapshot.Key;

				// Insert into the correct location, based on previousChildName
				if (previousChildName == null)
				{
					outerInstance.mModels.Insert(0, model);
					outerInstance.mKeys.Insert(0, key);
				}
				else
				{
					int previousIndex = outerInstance.mKeys.IndexOf(previousChildName);
					int nextIndex = previousIndex + 1;
					if (nextIndex == outerInstance.mModels.Count)
					{
						outerInstance.mModels.Add(model);
						outerInstance.mKeys.Add(key);
					}
					else
					{
						outerInstance.mModels.Insert(nextIndex, model);
						outerInstance.mKeys.Insert(nextIndex, key);
					}
				}

				notifyDataSetChanged();
			}

			public override void onChildChanged(DataSnapshot dataSnapshot, string s)
			{
				// One of the mModels changed. Replace it in our list and name mapping
				string key = dataSnapshot.Key;
				T newModel = dataSnapshot.getValue(outerInstance.mModelClass);
				int index = outerInstance.mKeys.IndexOf(key);

				outerInstance.mModels[index] = newModel;

				notifyDataSetChanged();
			}

			public override void onChildRemoved(DataSnapshot dataSnapshot)
			{

				// A model was removed from the list. Remove it from our list and the name mapping
				string key = dataSnapshot.Key;
				int index = outerInstance.mKeys.IndexOf(key);

				outerInstance.mKeys.RemoveAt(index);
				outerInstance.mModels.RemoveAt(index);

				notifyDataSetChanged();
			}

			public override void onChildMoved(DataSnapshot dataSnapshot, string previousChildName)
			{

				// A model changed position in the list. Update our list accordingly
				string key = dataSnapshot.Key;
				T newModel = dataSnapshot.getValue(outerInstance.mModelClass);
				int index = outerInstance.mKeys.IndexOf(key);
				outerInstance.mModels.RemoveAt(index);
				outerInstance.mKeys.RemoveAt(index);
				if (previousChildName == null)
				{
					outerInstance.mModels.Insert(0, newModel);
					outerInstance.mKeys.Insert(0, key);
				}
				else
				{
					int previousIndex = outerInstance.mKeys.IndexOf(previousChildName);
					int nextIndex = previousIndex + 1;
					if (nextIndex == outerInstance.mModels.Count)
					{
						outerInstance.mModels.Add(newModel);
						outerInstance.mKeys.Add(key);
					}
					else
					{
						outerInstance.mModels.Insert(nextIndex, newModel);
						outerInstance.mKeys.Insert(nextIndex, key);
					}
				}
				notifyDataSetChanged();
			}

			public override void onCancelled(FirebaseError firebaseError)
			{
				Log.e("FirebaseListAdapter", "Listen was cancelled, no more updates will occur");
			}

		}

		public virtual void cleanup()
		{
			// We're being destroyed, let go of our mListener and forget about all of the mModels
			mRef.removeEventListener(mListener);
			mModels.Clear();
			mKeys.Clear();
		}

		public override int Count
		{
			get
			{
				return mModels.Count;
			}
		}

		public override object getItem(int i)
		{
			return mModels[i];
		}

		public override long getItemId(int i)
		{
			return i;
		}

		public override View getView(int i, View view, ViewGroup viewGroup)
		{
			if (view == null)
			{
				view = mInflater.inflate(mLayout, viewGroup, false);
			}

			T model = mModels[i];
			// Call out to subclass to marshall this model into the provided view
			populateView(view, model);
			return view;
		}

		/// <summary>
		/// Each time the data at the given Firebase location changes, this method will be called for each item that needs
		/// to be displayed. The arguments correspond to the mLayout and mModelClass given to the constructor of this class.
		/// <p/>
		/// Your implementation should populate the view using the data contained in the model.
		/// </summary>
		/// <param name="v">     The view to populate </param>
		/// <param name="model"> The object containing the data used to populate the view </param>
		protected internal abstract void populateView(View v, T model);
	}

}