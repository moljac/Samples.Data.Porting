using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	/*
	 * Copyright (C) 2014 The Android Open Source Project
	 *
	 * Licensed under the Apache License, Version 2.0 (the "License");
	 * you may not use this file except in compliance with the License.
	 * You may obtain a copy of the License at
	 *
	 *      http://www.apache.org/licenses/LICENSE-2.0
	 *
	 * Unless required by applicable law or agreed to in writing, software
	 * distributed under the License is distributed on an "AS IS" BASIS,
	 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	 * See the License for the specific language governing permissions and
	 * limitations under the License.
	 *
	 *
	 * Custom changes from the original DefaultItemAnimator by AOSP:
	 *
	 * - package of the class
	 * - visibility of methods (private -> protected)
	 * - visibility of fields (private -> protected)
	 * - visibility of internal classes (private -> package)
	 * - onPreEndAnimation, onPostEndAnimation methods to plug-in the logic of derived classes
	 */

	using AnimatorCompatHelper = android.support.v4.animation.AnimatorCompatHelper;
	using ViewCompat = android.support.v4.view.ViewCompat;
	using ViewPropertyAnimatorCompat = android.support.v4.view.ViewPropertyAnimatorCompat;
	using ViewPropertyAnimatorListener = android.support.v4.view.ViewPropertyAnimatorListener;
	using RecyclerView = android.support.v7.widget.RecyclerView;
	using ViewHolder = android.support.v7.widget.RecyclerView.ViewHolder;
	using Log = android.util.Log;
	using View = android.view.View;


	/// <summary>
	/// This implementation of <seealso cref="RecyclerView.ItemAnimator"/> provides basic
	/// animations on remove, add, and move events that happen to the items in
	/// a RecyclerView. RecyclerView uses a DefaultItemAnimator by default.
	/// </summary>
	/// <seealso cref= RecyclerView#setItemAnimator(RecyclerView.ItemAnimator) </seealso>
	internal class DefaultItemAnimator : RecyclerView.ItemAnimator
	{
		private const bool DEBUG = false;
		private static readonly string TAG = typeof(DefaultItemAnimator).Name;

		protected internal List<RecyclerView.ViewHolder> mPendingRemovals = new List<RecyclerView.ViewHolder>();
		protected internal List<RecyclerView.ViewHolder> mPendingAdditions = new List<RecyclerView.ViewHolder>();
		protected internal List<MoveInfo> mPendingMoves = new List<MoveInfo>();
		protected internal List<ChangeInfo> mPendingChanges = new List<ChangeInfo>();

		private List<List<RecyclerView.ViewHolder>> mAdditionsList = new List<List<RecyclerView.ViewHolder>>();
		private List<List<MoveInfo>> mMovesList = new List<List<MoveInfo>>();
		private List<List<ChangeInfo>> mChangesList = new List<List<ChangeInfo>>();

		protected internal List<RecyclerView.ViewHolder> mAddAnimations = new List<RecyclerView.ViewHolder>();
		protected internal List<RecyclerView.ViewHolder> mMoveAnimations = new List<RecyclerView.ViewHolder>();
		protected internal List<RecyclerView.ViewHolder> mRemoveAnimations = new List<RecyclerView.ViewHolder>();
		protected internal List<RecyclerView.ViewHolder> mChangeAnimations = new List<RecyclerView.ViewHolder>();

		internal class MoveInfo
		{
			public RecyclerView.ViewHolder holder;
			public int fromX, fromY, toX, toY;

			internal MoveInfo(RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY)
			{
				this.holder = holder;
				this.fromX = fromX;
				this.fromY = fromY;
				this.toX = toX;
				this.toY = toY;
			}
		}

		internal class ChangeInfo
		{
			public RecyclerView.ViewHolder oldHolder, newHolder;
			public int fromX, fromY, toX, toY;
			internal ChangeInfo(RecyclerView.ViewHolder oldHolder, RecyclerView.ViewHolder newHolder)
			{
				this.oldHolder = oldHolder;
				this.newHolder = newHolder;
			}

			internal ChangeInfo(RecyclerView.ViewHolder oldHolder, RecyclerView.ViewHolder newHolder, int fromX, int fromY, int toX, int toY) : this(oldHolder, newHolder)
			{
				this.fromX = fromX;
				this.fromY = fromY;
				this.toX = toX;
				this.toY = toY;
			}

			public override string ToString()
			{
				return "ChangeInfo{" + "fromX=" + fromX + ", fromY=" + fromY + ", toX=" + toX + ", toY=" + toY + '}';
	//                    "oldHolder=" + oldHolder +
	//                    ", newHolder=" + newHolder +
			}

			public virtual bool hasNoTranslation()
			{
				return (fromX - toX) == 0 && (fromY - toY) == 0;
			}
		}

		public override void runPendingAnimations()
		{
			bool removalsPending = mPendingRemovals.Count > 0;
			bool movesPending = mPendingMoves.Count > 0;
			bool changesPending = mPendingChanges.Count > 0;
			bool additionsPending = mPendingAdditions.Count > 0;
			if (!removalsPending && !movesPending && !additionsPending && !changesPending)
			{
				// nothing to animate
				return;
			}
			// First, remove stuff
			foreach (RecyclerView.ViewHolder holder in mPendingRemovals)
			{
				animateRemoveImpl(holder);
			}
			mPendingRemovals.Clear();
			// Next, move stuff
			if (movesPending)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.ArrayList<MoveInfo> moves = new java.util.ArrayList<MoveInfo>();
				List<MoveInfo> moves = new List<MoveInfo>();
				moves.AddRange(mPendingMoves);
				mMovesList.Add(moves);
				mPendingMoves.Clear();
				Runnable mover = () =>
				{
					foreach (MoveInfo moveInfo in moves)
					{
						animateMoveImpl(moveInfo.holder, moveInfo.fromX, moveInfo.fromY, moveInfo.toX, moveInfo.toY);
					}
					moves.Clear();
					mMovesList.Remove(moves);
				};
				if (removalsPending)
				{
					View view = moves[0].holder.itemView;
					ViewCompat.postOnAnimationDelayed(view, mover, RemoveDuration);
				}
				else
				{
					mover.run();
				}
			}
			// Next, change stuff, to run in parallel with move animations
			if (changesPending)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.ArrayList<ChangeInfo> changes = new java.util.ArrayList<ChangeInfo>();
				List<ChangeInfo> changes = new List<ChangeInfo>();
				changes.AddRange(mPendingChanges);
				mChangesList.Add(changes);
				mPendingChanges.Clear();
				Runnable changer = () =>
				{
					foreach (ChangeInfo change in changes)
					{
						animateChangeImpl(change);
					}
					changes.Clear();
					mChangesList.Remove(changes);
				};
				if (removalsPending)
				{
					RecyclerView.ViewHolder holder = changes[0].oldHolder;
					ViewCompat.postOnAnimationDelayed(holder.itemView, changer, RemoveDuration);
				}
				else
				{
					changer.run();
				}
			}
			// Next, add stuff
			if (additionsPending)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.ArrayList<android.support.v7.widget.RecyclerView.ViewHolder> additions = new java.util.ArrayList<android.support.v7.widget.RecyclerView.ViewHolder>();
				List<RecyclerView.ViewHolder> additions = new List<RecyclerView.ViewHolder>();
				additions.AddRange(mPendingAdditions);
				mAdditionsList.Add(additions);
				mPendingAdditions.Clear();
				Runnable adder = () =>
				{
					foreach (RecyclerView.ViewHolder holder in additions)
					{
						animateAddImpl(holder);
					}
					additions.Clear();
					mAdditionsList.Remove(additions);
				};
				if (removalsPending || movesPending || changesPending)
				{
					long removeDuration = removalsPending ? RemoveDuration : 0;
					long moveDuration = movesPending ? MoveDuration : 0;
					long changeDuration = changesPending ? ChangeDuration : 0;
					long totalDelay = removeDuration + Math.Max(moveDuration, changeDuration);
					View view = additions[0].itemView;
					ViewCompat.postOnAnimationDelayed(view, adder, totalDelay);
				}
				else
				{
					adder.run();
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public boolean animateRemove(final android.support.v7.widget.RecyclerView.ViewHolder holder)
		public override bool animateRemove(RecyclerView.ViewHolder holder)
		{
			Log.d(TAG, "animateRemove(" + holder + ")");
			resetAnimation(holder);
			mPendingRemovals.Add(holder);
			return true;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected void animateRemoveImpl(final android.support.v7.widget.RecyclerView.ViewHolder holder)
		protected internal virtual void animateRemoveImpl(RecyclerView.ViewHolder holder)
		{
			Log.d(TAG, "animateRemoveImpl(" + holder + ")");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = holder.itemView;
			View view = holder.itemView;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v4.view.ViewPropertyAnimatorCompat animation = android.support.v4.view.ViewCompat.animate(view);
			ViewPropertyAnimatorCompat animation = ViewCompat.animate(view);
			mRemoveAnimations.Add(holder);
			animation.setDuration(RemoveDuration).alpha(0).setListener(new VpaListenerAdapterAnonymousInnerClassHelper(this, holder, view, animation))
		   .start();
		}

		private class VpaListenerAdapterAnonymousInnerClassHelper : VpaListenerAdapter
		{
			private readonly DefaultItemAnimator outerInstance;

			private RecyclerView.ViewHolder holder;
			private View view;
			private ViewPropertyAnimatorCompat animation;

			public VpaListenerAdapterAnonymousInnerClassHelper(DefaultItemAnimator outerInstance, RecyclerView.ViewHolder holder, View view, ViewPropertyAnimatorCompat animation)
			{
				this.outerInstance = outerInstance;
				this.holder = holder;
				this.view = view;
				this.animation = animation;
			}

			public override void onAnimationStart(View view)
			{
				dispatchRemoveStarting(holder);
			}

			public override void onAnimationEnd(View view)
			{
				animation.Listener = null;
				ViewCompat.setAlpha(view, 1);
				dispatchRemoveFinished(holder);
				outerInstance.mRemoveAnimations.Remove(holder);
				outerInstance.dispatchFinishedWhenDone();
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public boolean animateAdd(final android.support.v7.widget.RecyclerView.ViewHolder holder)
		public override bool animateAdd(RecyclerView.ViewHolder holder)
		{
			resetAnimation(holder);
			ViewCompat.setAlpha(holder.itemView, 0);
			mPendingAdditions.Add(holder);
			return true;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected void animateAddImpl(final android.support.v7.widget.RecyclerView.ViewHolder holder)
		protected internal virtual void animateAddImpl(RecyclerView.ViewHolder holder)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = holder.itemView;
			View view = holder.itemView;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v4.view.ViewPropertyAnimatorCompat animation = android.support.v4.view.ViewCompat.animate(view);
			ViewPropertyAnimatorCompat animation = ViewCompat.animate(view);
			mAddAnimations.Add(holder);
			animation.alpha(1).setDuration(AddDuration).setListener(new VpaListenerAdapterAnonymousInnerClassHelper2(this, holder, view, animation))
				   .start();
		}

		private class VpaListenerAdapterAnonymousInnerClassHelper2 : VpaListenerAdapter
		{
			private readonly DefaultItemAnimator outerInstance;

			private RecyclerView.ViewHolder holder;
			private View view;
			private ViewPropertyAnimatorCompat animation;

			public VpaListenerAdapterAnonymousInnerClassHelper2(DefaultItemAnimator outerInstance, RecyclerView.ViewHolder holder, View view, ViewPropertyAnimatorCompat animation)
			{
				this.outerInstance = outerInstance;
				this.holder = holder;
				this.view = view;
				this.animation = animation;
			}

			public override void onAnimationStart(View view)
			{
				dispatchAddStarting(holder);
			}

			public override void onAnimationCancel(View view)
			{
				ViewCompat.setAlpha(view, 1);
			}

			public override void onAnimationEnd(View view)
			{
				animation.Listener = null;
				dispatchAddFinished(holder);
				outerInstance.mAddAnimations.Remove(holder);
				outerInstance.dispatchFinishedWhenDone();
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public boolean animateMove(final android.support.v7.widget.RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY)
		public override bool animateMove(RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = holder.itemView;
			View view = holder.itemView;
			fromX += ViewCompat.getTranslationX(holder.itemView);
			fromY += ViewCompat.getTranslationY(holder.itemView);
			resetAnimation(holder);
			int deltaX = toX - fromX;
			int deltaY = toY - fromY;
			if (deltaX == 0 && deltaY == 0)
			{
				dispatchMoveFinished(holder);
				return false;
			}
			if (deltaX != 0)
			{
				ViewCompat.setTranslationX(view, -deltaX);
			}
			if (deltaY != 0)
			{
				ViewCompat.setTranslationY(view, -deltaY);
			}
			mPendingMoves.Add(new MoveInfo(holder, fromX, fromY, toX, toY));
			return true;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected void animateMoveImpl(final android.support.v7.widget.RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY)
		protected internal virtual void animateMoveImpl(RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = holder.itemView;
			View view = holder.itemView;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int deltaX = toX - fromX;
			int deltaX = toX - fromX;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int deltaY = toY - fromY;
			int deltaY = toY - fromY;
			if (deltaX != 0)
			{
				ViewCompat.animate(view).translationX(0);
			}
			if (deltaY != 0)
			{
				ViewCompat.animate(view).translationY(0);
			}
			// TODO: make EndActions end listeners instead, since end actions aren't called when
			// vpas are canceled (and can't end them. why?)
			// need listener functionality in VPACompat for this. Ick.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v4.view.ViewPropertyAnimatorCompat animation = android.support.v4.view.ViewCompat.animate(view);
			ViewPropertyAnimatorCompat animation = ViewCompat.animate(view);
			mMoveAnimations.Add(holder);
			animation.setDuration(MoveDuration).setListener(new VpaListenerAdapterAnonymousInnerClassHelper3(this, holder, view, deltaX, deltaY, animation))
		   .start();
		}

		private class VpaListenerAdapterAnonymousInnerClassHelper3 : VpaListenerAdapter
		{
			private readonly DefaultItemAnimator outerInstance;

			private RecyclerView.ViewHolder holder;
			private View view;
			private int deltaX;
			private int deltaY;
			private ViewPropertyAnimatorCompat animation;

			public VpaListenerAdapterAnonymousInnerClassHelper3(DefaultItemAnimator outerInstance, RecyclerView.ViewHolder holder, View view, int deltaX, int deltaY, ViewPropertyAnimatorCompat animation)
			{
				this.outerInstance = outerInstance;
				this.holder = holder;
				this.view = view;
				this.deltaX = deltaX;
				this.deltaY = deltaY;
				this.animation = animation;
			}

			public override void onAnimationStart(View view)
			{
				dispatchMoveStarting(holder);
			}

			public override void onAnimationCancel(View view)
			{
				if (deltaX != 0)
				{
					ViewCompat.setTranslationX(view, 0);
				}
				if (deltaY != 0)
				{
					ViewCompat.setTranslationY(view, 0);
				}
			}

			public override void onAnimationEnd(View view)
			{
				animation.Listener = null;
				dispatchMoveFinished(holder);
				outerInstance.mMoveAnimations.Remove(holder);
				outerInstance.dispatchFinishedWhenDone();
			}
		}

		public override bool animateChange(RecyclerView.ViewHolder oldHolder, RecyclerView.ViewHolder newHolder, int fromX, int fromY, int toX, int toY)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float prevTranslationX = android.support.v4.view.ViewCompat.getTranslationX(oldHolder.itemView);
			float prevTranslationX = ViewCompat.getTranslationX(oldHolder.itemView);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float prevTranslationY = android.support.v4.view.ViewCompat.getTranslationY(oldHolder.itemView);
			float prevTranslationY = ViewCompat.getTranslationY(oldHolder.itemView);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float prevAlpha = android.support.v4.view.ViewCompat.getAlpha(oldHolder.itemView);
			float prevAlpha = ViewCompat.getAlpha(oldHolder.itemView);
			resetAnimation(oldHolder);
			int deltaX = (int)(toX - fromX - prevTranslationX);
			int deltaY = (int)(toY - fromY - prevTranslationY);
			// recover prev translation state after ending animation
			ViewCompat.setTranslationX(oldHolder.itemView, prevTranslationX);
			ViewCompat.setTranslationY(oldHolder.itemView, prevTranslationY);
			ViewCompat.setAlpha(oldHolder.itemView, prevAlpha);
			if (newHolder != null && newHolder.itemView != null)
			{
				// carry over translation values
				resetAnimation(newHolder);
				ViewCompat.setTranslationX(newHolder.itemView, -deltaX);
				ViewCompat.setTranslationY(newHolder.itemView, -deltaY);
				ViewCompat.setAlpha(newHolder.itemView, 0);
			}
			mPendingChanges.Add(new ChangeInfo(oldHolder, newHolder, fromX, fromY, toX, toY));
			return true;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected void animateChangeImpl(final ChangeInfo changeInfo)
		protected internal virtual void animateChangeImpl(ChangeInfo changeInfo)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v7.widget.RecyclerView.ViewHolder holder = changeInfo.oldHolder;
			RecyclerView.ViewHolder holder = changeInfo.oldHolder;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = holder == null ? null : holder.itemView;
			View view = holder == null ? null : holder.itemView;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v7.widget.RecyclerView.ViewHolder newHolder = changeInfo.newHolder;
			RecyclerView.ViewHolder newHolder = changeInfo.newHolder;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View newView = newHolder != null ? newHolder.itemView : null;
			View newView = newHolder != null ? newHolder.itemView : null;
			if (view != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v4.view.ViewPropertyAnimatorCompat oldViewAnim = android.support.v4.view.ViewCompat.animate(view).setDuration(getChangeDuration());
				ViewPropertyAnimatorCompat oldViewAnim = ViewCompat.animate(view).setDuration(ChangeDuration);
				mChangeAnimations.Add(changeInfo.oldHolder);
				oldViewAnim.translationX(changeInfo.toX - changeInfo.fromX);
				oldViewAnim.translationY(changeInfo.toY - changeInfo.fromY);
				oldViewAnim.alpha(0).setListener(new VpaListenerAdapterAnonymousInnerClassHelper4(this, changeInfo, view, oldViewAnim))
			   .start();
			}
			if (newView != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v4.view.ViewPropertyAnimatorCompat newViewAnimation = android.support.v4.view.ViewCompat.animate(newView);
				ViewPropertyAnimatorCompat newViewAnimation = ViewCompat.animate(newView);
				mChangeAnimations.Add(changeInfo.newHolder);
				newViewAnimation.translationX(0).translationY(0).setDuration(ChangeDuration).alpha(1).setListener(new VpaListenerAdapterAnonymousInnerClassHelper5(this, changeInfo, view, newView, newViewAnimation))
			   .start();
			}
		}

		private class VpaListenerAdapterAnonymousInnerClassHelper4 : VpaListenerAdapter
		{
			private readonly DefaultItemAnimator outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.DefaultItemAnimator.ChangeInfo changeInfo;
			private View view;
			private ViewPropertyAnimatorCompat oldViewAnim;

			public VpaListenerAdapterAnonymousInnerClassHelper4(DefaultItemAnimator outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.DefaultItemAnimator.ChangeInfo changeInfo, View view, ViewPropertyAnimatorCompat oldViewAnim)
			{
				this.outerInstance = outerInstance;
				this.changeInfo = changeInfo;
				this.view = view;
				this.oldViewAnim = oldViewAnim;
			}

			public override void onAnimationStart(View view)
			{
				dispatchChangeStarting(changeInfo.oldHolder, true);
			}

			public override void onAnimationEnd(View view)
			{
				oldViewAnim.Listener = null;
				ViewCompat.setAlpha(view, 1);
				ViewCompat.setTranslationX(view, 0);
				ViewCompat.setTranslationY(view, 0);
				dispatchChangeFinished(changeInfo.oldHolder, true);
				outerInstance.mChangeAnimations.Remove(changeInfo.oldHolder);
				outerInstance.dispatchFinishedWhenDone();
			}
		}

		private class VpaListenerAdapterAnonymousInnerClassHelper5 : VpaListenerAdapter
		{
			private readonly DefaultItemAnimator outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.DefaultItemAnimator.ChangeInfo changeInfo;
			private View view;
			private View newView;
			private ViewPropertyAnimatorCompat newViewAnimation;

			public VpaListenerAdapterAnonymousInnerClassHelper5(DefaultItemAnimator outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.DefaultItemAnimator.ChangeInfo changeInfo, View view, View newView, ViewPropertyAnimatorCompat newViewAnimation)
			{
				this.outerInstance = outerInstance;
				this.changeInfo = changeInfo;
				this.view = view;
				this.newView = newView;
				this.newViewAnimation = newViewAnimation;
			}

			public override void onAnimationStart(View view)
			{
				dispatchChangeStarting(changeInfo.newHolder, false);
			}
			public override void onAnimationEnd(View view)
			{
				newViewAnimation.Listener = null;
				ViewCompat.setAlpha(newView, 1);
				ViewCompat.setTranslationX(newView, 0);
				ViewCompat.setTranslationY(newView, 0);
				dispatchChangeFinished(changeInfo.newHolder, false);
				outerInstance.mChangeAnimations.Remove(changeInfo.newHolder);
				outerInstance.dispatchFinishedWhenDone();
			}
		}

		protected internal virtual void endChangeAnimation(IList<ChangeInfo> infoList, RecyclerView.ViewHolder item)
		{
			for (int i = infoList.Count - 1; i >= 0; i--)
			{
				ChangeInfo changeInfo = infoList[i];
				if (endChangeAnimationIfNecessary(changeInfo, item))
				{
					if (changeInfo.oldHolder == null && changeInfo.newHolder == null)
					{
						infoList.Remove(changeInfo);
					}
				}
			}
		}

		private void endChangeAnimationIfNecessary(ChangeInfo changeInfo)
		{
			if (changeInfo.oldHolder != null)
			{
				endChangeAnimationIfNecessary(changeInfo, changeInfo.oldHolder);
			}
			if (changeInfo.newHolder != null)
			{
				endChangeAnimationIfNecessary(changeInfo, changeInfo.newHolder);
			}
		}
		private bool endChangeAnimationIfNecessary(ChangeInfo changeInfo, RecyclerView.ViewHolder item)
		{
			bool oldItem = false;
			if (changeInfo.newHolder == item)
			{
				changeInfo.newHolder = null;
			}
			else if (changeInfo.oldHolder == item)
			{
				changeInfo.oldHolder = null;
				oldItem = true;
			}
			else
			{
				return false;
			}
			ViewCompat.setAlpha(item.itemView, 1);
			ViewCompat.setTranslationX(item.itemView, 0);
			ViewCompat.setTranslationY(item.itemView, 0);
			dispatchChangeFinished(item, oldItem);
			return true;
		}

		public override void endAnimation(RecyclerView.ViewHolder item)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = item.itemView;
			View view = item.itemView;
			// this will trigger end callback which should set properties to their target values.
			ViewCompat.animate(view).cancel();

			onPreEndAnimation(item);

			// TODO if some other animations are chained to end, how do we cancel them as well?
			for (int i = mPendingMoves.Count - 1; i >= 0; i--)
			{
				MoveInfo moveInfo = mPendingMoves[i];
				if (moveInfo.holder == item)
				{
					ViewCompat.setTranslationY(view, 0);
					ViewCompat.setTranslationX(view, 0);
					dispatchMoveFinished(item);
					mPendingMoves.RemoveAt(i);
				}
			}
			endChangeAnimation(mPendingChanges, item);
			if (mPendingRemovals.Remove(item))
			{
				ViewCompat.setAlpha(view, 1);
				dispatchRemoveFinished(item);
			}
			if (mPendingAdditions.Remove(item))
			{
				ViewCompat.setAlpha(view, 1);
				dispatchAddFinished(item);
			}

			for (int i = mChangesList.Count - 1; i >= 0; i--)
			{
				List<ChangeInfo> changes = mChangesList[i];
				endChangeAnimation(changes, item);
				if (changes.Count == 0)
				{
					mChangesList.RemoveAt(i);
				}
			}
			for (int i = mMovesList.Count - 1; i >= 0; i--)
			{
				List<MoveInfo> moves = mMovesList[i];
				for (int j = moves.Count - 1; j >= 0; j--)
				{
					MoveInfo moveInfo = moves[j];
					if (moveInfo.holder == item)
					{
						ViewCompat.setTranslationY(view, 0);
						ViewCompat.setTranslationX(view, 0);
						dispatchMoveFinished(item);
						moves.RemoveAt(j);
						if (moves.Count == 0)
						{
							mMovesList.RemoveAt(i);
						}
						break;
					}
				}
			}
			for (int i = mAdditionsList.Count - 1; i >= 0; i--)
			{
				List<RecyclerView.ViewHolder> additions = mAdditionsList[i];
				if (additions.Remove(item))
				{
					ViewCompat.setAlpha(view, 1);
					dispatchAddFinished(item);
					if (additions.Count == 0)
					{
						mAdditionsList.RemoveAt(i);
					}
				}
			}

			onPostEndAnimation(item);

			// animations should be ended by the cancel above.
			if (mRemoveAnimations.Remove(item) && DEBUG)
			{
				throw new System.InvalidOperationException("after animation is cancelled, item should not be in " + "mRemoveAnimations list");
			}

			if (mAddAnimations.Remove(item) && DEBUG)
			{
				throw new System.InvalidOperationException("after animation is cancelled, item should not be in " + "mAddAnimations list");
			}

			if (mChangeAnimations.Remove(item) && DEBUG)
			{
				throw new System.InvalidOperationException("after animation is cancelled, item should not be in " + "mChangeAnimations list");
			}

			if (mMoveAnimations.Remove(item) && DEBUG)
			{
				throw new System.InvalidOperationException("after animation is cancelled, item should not be in " + "mMoveAnimations list");
			}
			dispatchFinishedWhenDone();
		}

		protected internal virtual void onPreEndAnimation(RecyclerView.ViewHolder item)
		{
			// NOOP
		}

		protected internal virtual void onPostEndAnimation(RecyclerView.ViewHolder item)
		{
			// NOOP
		}

		private void resetAnimation(RecyclerView.ViewHolder holder)
		{
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.v(TAG, "resetAnimation(" + holder + ")");
			}
			AnimatorCompatHelper.clearInterpolator(holder.itemView);
			endAnimation(holder);
		}

		public override bool Running
		{
			get
			{
				return (mPendingAdditions.Count > 0 || mPendingChanges.Count > 0 || mPendingMoves.Count > 0 || mPendingRemovals.Count > 0 || mMoveAnimations.Count > 0 || mRemoveAnimations.Count > 0 || mAddAnimations.Count > 0 || mChangeAnimations.Count > 0 || mMovesList.Count > 0 || mAdditionsList.Count > 0 || mChangesList.Count > 0);
			}
		}

		/// <summary>
		/// Check the state of currently pending and running animations. If there are none
		/// pending/running, call <seealso cref="#dispatchAnimationsFinished()"/> to notify any
		/// listeners.
		/// </summary>
		protected internal virtual void dispatchFinishedWhenDone()
		{
			if (!Running)
			{
				dispatchAnimationsFinished();
			}
		}

		public override void endAnimations()
		{
			int count = mPendingMoves.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				MoveInfo item = mPendingMoves[i];
				View view = item.holder.itemView;
				ViewCompat.setTranslationY(view, 0);
				ViewCompat.setTranslationX(view, 0);
				dispatchMoveFinished(item.holder);
				mPendingMoves.RemoveAt(i);
			}
			count = mPendingRemovals.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				RecyclerView.ViewHolder item = mPendingRemovals[i];
				dispatchRemoveFinished(item);
				mPendingRemovals.RemoveAt(i);
			}
			count = mPendingAdditions.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				RecyclerView.ViewHolder item = mPendingAdditions[i];
				View view = item.itemView;
				ViewCompat.setAlpha(view, 1);
				dispatchAddFinished(item);
				mPendingAdditions.RemoveAt(i);
			}
			count = mPendingChanges.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				endChangeAnimationIfNecessary(mPendingChanges[i]);
			}
			mPendingChanges.Clear();
			if (!Running)
			{
				return;
			}

			int listCount = mMovesList.Count;
			for (int i = listCount - 1; i >= 0; i--)
			{
				List<MoveInfo> moves = mMovesList[i];
				count = moves.Count;
				for (int j = count - 1; j >= 0; j--)
				{
					MoveInfo moveInfo = moves[j];
					RecyclerView.ViewHolder item = moveInfo.holder;
					View view = item.itemView;
					ViewCompat.setTranslationY(view, 0);
					ViewCompat.setTranslationX(view, 0);
					dispatchMoveFinished(moveInfo.holder);
					moves.RemoveAt(j);
					if (moves.Count == 0)
					{
						mMovesList.Remove(moves);
					}
				}
			}
			listCount = mAdditionsList.Count;
			for (int i = listCount - 1; i >= 0; i--)
			{
				List<RecyclerView.ViewHolder> additions = mAdditionsList[i];
				count = additions.Count;
				for (int j = count - 1; j >= 0; j--)
				{
					RecyclerView.ViewHolder item = additions[j];
					View view = item.itemView;
					ViewCompat.setAlpha(view, 1);
					dispatchAddFinished(item);
					additions.RemoveAt(j);
					if (additions.Count == 0)
					{
						mAdditionsList.Remove(additions);
					}
				}
			}
			listCount = mChangesList.Count;
			for (int i = listCount - 1; i >= 0; i--)
			{
				List<ChangeInfo> changes = mChangesList[i];
				count = changes.Count;
				for (int j = count - 1; j >= 0; j--)
				{
					endChangeAnimationIfNecessary(changes[j]);
					if (changes.Count == 0)
					{
						mChangesList.Remove(changes);
					}
				}
			}

			cancelAll(mRemoveAnimations);
			cancelAll(mMoveAnimations);
			cancelAll(mAddAnimations);
			cancelAll(mChangeAnimations);

			dispatchAnimationsFinished();
		}

		internal virtual void cancelAll(IList<RecyclerView.ViewHolder> viewHolders)
		{
			for (int i = viewHolders.Count - 1; i >= 0; i--)
			{
				ViewCompat.animate(viewHolders[i].itemView).cancel();
			}
		}

		internal class VpaListenerAdapter : ViewPropertyAnimatorListener
		{
			public override void onAnimationStart(View view)
			{
			}
			public override void onAnimationEnd(View view)
			{
			}
			public override void onAnimationCancel(View view)
			{
			}
		}
	}
}