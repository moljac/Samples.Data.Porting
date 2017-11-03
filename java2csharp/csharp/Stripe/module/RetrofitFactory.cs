namespace com.stripe.example.module
{

	using Gson = com.google.gson.Gson;
	using GsonBuilder = com.google.gson.GsonBuilder;

	using OkHttpClient = okhttp3.OkHttpClient;
	using HttpLoggingInterceptor = okhttp3.logging.HttpLoggingInterceptor;
	using Retrofit = retrofit2.Retrofit;
	using RxJavaCallAdapterFactory = retrofit2.adapter.rxjava.RxJavaCallAdapterFactory;
	using GsonConverterFactory = retrofit2.converter.gson.GsonConverterFactory;

	/// <summary>
	/// Factory to generate our Retrofit instance.
	/// </summary>
	public class RetrofitFactory
	{

		// Put your Base URL here. Unless you customized it, the URL will be something like
		// https://hidden-beach-12345.herokuapp.com/
		private const string BASE_URL = "put your url here";
		private static Retrofit mInstance = null;

		public static Retrofit Instance
		{
			get
			{
				if (mInstance == null)
				{
    
					HttpLoggingInterceptor logging = new HttpLoggingInterceptor();
					// Set your desired log level. Use Level.BODY for debugging errors.
					logging.Level = HttpLoggingInterceptor.Level.BODY;
    
					OkHttpClient.Builder httpClient = new OkHttpClient.Builder();
					httpClient.addInterceptor(logging);
    
					Gson gson = (new GsonBuilder()).setLenient().create();
    
					// Adding Rx so the calls can be Observable, and adding a Gson converter with
					// leniency to make parsing the results simple.
					mInstance = (new Retrofit.Builder()).addConverterFactory(GsonConverterFactory.create(gson)).addCallAdapterFactory(RxJavaCallAdapterFactory.create()).baseUrl(BASE_URL).client(httpClient.build()).build();
				}
				return mInstance;
			}
		}
	}

}