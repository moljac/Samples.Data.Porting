using System;
using System.Collections.Generic;

namespace com.stripe.example.service
{

	using NonNull = android.support.annotation.NonNull;
	using Size = android.support.annotation.Size;

	using EphemeralKeyProvider = com.stripe.android.EphemeralKeyProvider;
	using EphemeralKeyUpdateListener = com.stripe.android.EphemeralKeyUpdateListener;
	using RetrofitFactory = com.stripe.example.module.RetrofitFactory;


	using ResponseBody = okhttp3.ResponseBody;
	using Retrofit = retrofit2.Retrofit;
	using AndroidSchedulers = rx.android.schedulers.AndroidSchedulers;
	using Action1 = rx.functions.Action1;
	using Schedulers = rx.schedulers.Schedulers;
	using CompositeSubscription = rx.subscriptions.CompositeSubscription;

	/// <summary>
	/// An implementation of <seealso cref="EphemeralKeyProvider"/> that can be used to generate
	/// ephemeral keys on the backend.
	/// </summary>
	public class ExampleEphemeralKeyProvider : EphemeralKeyProvider
	{

		private  NonNull;
		private  NonNull;
		private  NonNull;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ExampleEphemeralKeyProvider(@NonNull ProgressListener progressListener)
		public ExampleEphemeralKeyProvider(ProgressListener progressListener)
		{
			Retrofit retrofit = RetrofitFactory.Instance;
			mStripeService = retrofit.create(typeof(StripeService));
			mCompositeSubscription = new CompositeSubscription();
			mProgressListener = progressListener;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void createEphemeralKey(@NonNull @Size(min = 4) String apiVersion, @NonNull final com.stripe.android.EphemeralKeyUpdateListener keyUpdateListener)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public override void createEphemeralKey(string apiVersion, EphemeralKeyUpdateListener keyUpdateListener)
		{
			IDictionary<string, string> apiParamMap = new Dictionary<string, string>();
			apiParamMap["api_version"] = apiVersion;

//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
//			mCompositeSubscription.add(mStripeService.createEphemeralKey(apiParamMap).subscribeOn(rx.schedulers.Schedulers.io()).observeOn(rx.android.schedulers.AndroidSchedulers.mainThread()).subscribe(new rx.functions.Action1<okhttp3.ResponseBody>()
	//		{
	//							@@Override public void call(ResponseBody response)
	//							{
	//								try
	//								{
	//									String rawKey = response.@string();
	//									keyUpdateListener.onKeyUpdate(rawKey);
	//									mProgressListener.onStringResponse(rawKey);
	//								}
	//								catch (IOException iox)
	//								{
	//
	//								}
	//							}
	//						}
						   , new Action1AnonymousInnerClassHelper2(this)
						   ));
		}

		private class Action1AnonymousInnerClassHelper2 : Action1<Exception>
		{
			private readonly ExampleEphemeralKeyProvider outerInstance;

			public Action1AnonymousInnerClassHelper2(ExampleEphemeralKeyProvider outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call(Exception throwable)
			{
				mProgressListener.onStringResponse(throwable.Message);
			}
		}

		public interface ProgressListener
		{
			void onStringResponse(string @string);
		}
	}

}