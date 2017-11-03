using System.Collections.Generic;

namespace com.stripe.example.service
{

	using ResponseBody = okhttp3.ResponseBody;
	using FieldMap = retrofit2.http.FieldMap;
	using FormUrlEncoded = retrofit2.http.FormUrlEncoded;
	using POST = retrofit2.http.POST;
	using Observable = rx.Observable;

	/// <summary>
	/// A Retrofit service used to communicate with a server.
	/// </summary>
	public interface StripeService
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @FormUrlEncoded @POST("ephemeral_keys") rx.Observable<okhttp3.ResponseBody> createEphemeralKey(@FieldMap java.util.Map<String, String> apiVersionMap);
		Observable<ResponseBody> createEphemeralKey(IDictionary<string, string> apiVersionMap);

	}

}