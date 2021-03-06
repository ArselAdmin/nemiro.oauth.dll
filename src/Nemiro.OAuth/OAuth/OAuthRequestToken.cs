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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Nemiro.OAuth.Extensions;

namespace Nemiro.OAuth
{

  /// <summary>
  /// Represents the request token results.
  /// </summary>
  public class OAuthRequestToken : RequestResult
  {

    #region ..fields & properties..

    private string _OAuthToken = "";

    /// <summary>
    /// Gets the OAuth token.
    /// </summary>
    public string OAuthToken
    {
      get
      {
        return _OAuthToken;
      }
    }

    private string _OAuthTokenSecret = "";

    /// <summary>
    /// Gets the token secret.
    /// </summary>
    public string OAuthTokenSecret
    {
      get
      {
        return _OAuthTokenSecret;
      }
    }

    private bool _OAuthCallbackConfirmed = false;

    /// <summary>
    /// The parameter is used to  differentiate from previous versions of the protocol.
    /// </summary>
    public bool OAuthCallbackConfirmed
    {
      get
      {
        return _OAuthCallbackConfirmed;
      }
    }

    private string _AuthorizationUrl = "";

    /// <summary>
    /// Gets the address of the authorization.
    /// </summary>
    public string AuthorizationUrl
    {
      get
      {
        return _AuthorizationUrl;
      }
    }

    #endregion
    #region ..constructor..

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthRequestToken"/> class.
    /// </summary>
    /// <param name="result">The request result.</param>
    /// <param name="authorizeUrl">The address of the authorization.</param>
    /// <param name="parameters">The query parameters. Will be used in the formation of the authorization address.</param>
    public OAuthRequestToken(RequestResult result, string authorizeUrl, NameValueCollection parameters) : base(result)
    {
      _OAuthToken = base["oauth_token"].ToString();
      _OAuthTokenSecret = base["oauth_token_secret"].ToString();
      _OAuthCallbackConfirmed = Convert.ToBoolean(base["oauth_callback_confirmed"]);
      _AuthorizationUrl = authorizeUrl;
      _AuthorizationUrl += _AuthorizationUrl.EndsWith("?") ? "&" : "?";
      _AuthorizationUrl += String.Format("oauth_token={0}", OAuthUtility.UrlEncode(_OAuthToken));
      if (parameters != null && parameters.Count > 0)
      {
        _AuthorizationUrl += "&" + parameters.ToParametersString("&");
      }
    }

    #endregion
    #region ..methods..

    /// <summary>
    /// Returns the <see cref="OAuthRequestToken.OAuthToken"/>.
    /// </summary>
    public override string ToString()
    {
      return this.OAuthToken;
    }

    #endregion

  }
}
