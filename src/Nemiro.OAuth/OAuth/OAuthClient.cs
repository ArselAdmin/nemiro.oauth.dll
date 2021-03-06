﻿// ----------------------------------------------------------------------------
// Copyright (c) Aleksey Nemiro, 2014. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------
using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Collections.Generic;
using Nemiro.OAuth.Extensions;

namespace Nemiro.OAuth
{

  /// <summary>
  /// Represents base properties and method for OAuth 1.0 client.
  /// </summary>
  /// <remarks>
  /// <para>For more details, please visit <see href="http://tools.ietf.org/html/rfc5849"/>.</para>
  /// </remarks>
  public abstract class OAuthClient : OAuthBase
  {

    #region ..fields & properties..

    /// <summary>
    /// Gets or sets the address for the request token.
    /// </summary>
    protected string RequestTokenUrl { get; set; }

    private OAuthAuthorization _Authorization = new OAuthAuthorization();

    /// <summary>
    /// Get the authorization parameters.
    /// </summary>
    public OAuthAuthorization Authorization
    {
      get
      {
        return _Authorization;
      }
    }

    /// <summary>
    /// Gets the endpoint of the authorization.
    /// </summary>
    public override string AuthorizationUrl
    {
      get
      {
        return this.RequestToken.AuthorizationUrl;
      }
    }

    private OAuthRequestToken _RequestToken = null;
    
    /// <summary>
    /// Gets or sets the request token.
    /// </summary>
    public OAuthRequestToken RequestToken
    {
      get
      {
        if (_RequestToken == null)
        {
          this.GetRequestToken();
        }
        return _RequestToken;
      }
      protected internal set
      {
        _RequestToken = value;
      }
    }

    #endregion
    #region ..constructor..

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthClient"/> class.
    /// </summary>
    /// <param name="requestTokenUrl">The address for the request token.</param>
    /// <param name="authorizeUrl">The address for login.</param>
    /// <param name="accessTokenUrl">The address for the access token.</param>
    /// <param name="consumerKey">The application identifier.</param>
    /// <param name="consumerSecret">The application secret key.</param>
    /// <param name="signatureMethod">The name of hashing algorithm to calculate the signature: HMAC-SHA1 (default) or PLAINTEXT.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="requestTokenUrl"/> is null or empty.</exception>
    public OAuthClient(string requestTokenUrl, string authorizeUrl, string accessTokenUrl, string consumerKey, string consumerSecret, string signatureMethod = SignatureMethods.HMACSHA1) : base(authorizeUrl, accessTokenUrl, consumerKey, consumerSecret)
    {
      if (String.IsNullOrEmpty(requestTokenUrl)) { throw new ArgumentNullException("requestTokenUrl"); }

      this.RequestTokenUrl = requestTokenUrl;

      this.Authorization["oauth_consumer_key"] = consumerKey;
      this.Authorization["oauth_nonce"] = OAuthUtility.GetRandomKey();
      this.Authorization["oauth_signature"] = "";
      this.Authorization["oauth_signature_method"] = signatureMethod;
      this.Authorization["oauth_timestamp"] = OAuthUtility.GetTimeStamp();
      this.Authorization["oauth_token"] = "";
      this.Authorization["oauth_version"] = "1.0";
    }

    #endregion
    #region ..methods..

    /// <summary>
    /// Gets base string of the signature for current request.
    /// </summary>
    /// <remarks><para>For more details, please visit <see href="http://tools.ietf.org/html/rfc5849#section-3.4.1.1"/></para></remarks>
    [Obsolete("Use OAuthUtility.", false)]
    private string GetSignatureBaseString(string httpMethod, Uri url, NameValueCollection parameters)
    {
      return OAuthUtility.GetSignatureBaseString(httpMethod, url, parameters, this.Authorization);
    }

    /// <summary>
    /// Gets signature for the current request.
    /// </summary>
    /// <param name="httpMethod">The HTTP method: <b>GET</b> or <b>POST</b>. Default is <b>POST</b>.</param>
    /// <param name="url">The request URI.</param>
    /// <param name="tokenSecret">The token secret.</param>
    /// <param name="parameters">The query parameters.</param>
    [Obsolete("Use OAuthUtility.", false)]
    public OAuthSignature GetSignature(string httpMethod, Uri url, string tokenSecret, NameValueCollection parameters)
    {
      return OAuthUtility.GetSignature(httpMethod, url, this.ApplicationSecret, tokenSecret, parameters, this.Authorization);
    }

    /// <summary>
    /// Gets the request token from the remote server.
    /// </summary>
    public void GetRequestToken()
    {
      this.UpdateStamp();

      if (!String.IsNullOrEmpty(this.ReturnUrl))
      {
        this.Authorization["oauth_callback"] = String.Format
        (
          "{0}{1}state={2}", 
          this.ReturnUrl, 
          (this.ReturnUrl.Contains("?") ? "&" : "?"),
          this.State
        );
      }
      else
      {
        this.Authorization.Parameters.Remove("oauth_callback");
      }

      this.Authorization.SetSignature("POST", new Uri(this.RequestTokenUrl), this.ApplicationSecret, "", null);
      //this.Authorization["oauth_signature"] = this.GetSignature("POST", new Uri(this.RequestTokenUrl), "", null);

      _RequestToken = new OAuthRequestToken
      (
        OAuthUtility.ExecuteRequest
        (
          "POST", 
          this.RequestTokenUrl, 
          null, 
          this.Authorization.ToString()
        ), 
        this.AuthorizeUrl,
        this.Parameters
      ); 
    }

    /// <summary>
    /// Gets the access token from the remote server.
    /// </summary>
    protected override void GetAccessToken()
    {
      base.GetAccessToken();
      
      this.Authorization.Parameters.Remove("oauth_signature");

      this.Authorization["oauth_nonce"] = OAuthUtility.GetRandomKey();
      this.Authorization["oauth_timestamp"] = OAuthUtility.GetTimeStamp();
      this.Authorization["oauth_verifier"] =this.AuthorizationCode;
      this.Authorization["oauth_token"] = this.RequestToken.OAuthToken;
      this.Authorization.SetSignature
      (
        "POST",
        new Uri(this.AccessTokenUrl),
        this.ApplicationSecret,
        this.Authorization["oauth_token"].ToString(),
        null
      );

      base.AccessToken = new OAuthAccessToken
      (
        OAuthUtility.ExecuteRequest
        (
          "POST", 
          this.AccessTokenUrl, 
          null, 
          this.Authorization.ToString()
        )
      );

      this.Authorization.Parameters.Remove("oauth_verifier");
    }

    /// <summary>
    /// Updates the nonce and timestamp.
    /// </summary>
    private void UpdateStamp()
    {
      this.Authorization["oauth_nonce"] = OAuthUtility.GetRandomKey();
      this.Authorization["oauth_timestamp"] = OAuthUtility.GetTimeStamp();
    }

    #endregion

  }

}