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
using System.Net;
using System.IO;
using System.Collections.Specialized;

// If it works, no need to change the code. 
// Just use it! ;-)

namespace Nemiro.OAuth.Clients
{

  /// <summary>
  /// OAuth client for <b>Yahoo</b>.
  /// </summary>
  /// <remarks>
  /// <h1>Register and Configure a Yahoo Application</h1>
  /// <list type="table">
  /// <item>
  /// <term><img src="../img/warning.png" alt="(!)" title="" /></term>
  /// <term>
  /// <b>Web Management Interface may change over time. Applications registration shown below may differ.</b><br />
  /// If the interface is changed, you need to register the application and get <b>Consumer ID</b> and <b>Consumer Secret</b>. For web projects, configure <b>return URLs</b>.<br />
  /// If you have any problems with this, please <see href="https://github.com/alekseynemiro/nemiro.oauth.dll/issues">visit issues</see>. If you do not find a solution to your problem, you can <see href="https://github.com/alekseynemiro/nemiro.oauth.dll/issues/new">create a new question</see>.
  /// </term>
  /// </item>
  /// </list>
  /// <para>Open <b><see href="https://developer.yahoo.com/">Yahoo Developer Network</see></b> and <b><see href="https://developer.apps.yahoo.com/dashboard/createKey.html">Create a Project</see></b>.</para>
  /// <para>
  /// In the application settings  you can found <b>Consumer Key</b> and <b>Consumer Secret</b>.
  /// Use this for creating an instance of the <see cref="YahooClient"/> class.
  /// </para>
  /// <para>Note that <b>Yahoo!</b> does not work with the localhost. Use only a real servers. Make sure that your application on the Yahoo! dashboard configured correctly.</para>
  /// <para><b>Yahoo!</b> has a pretty flimsy OAuth interface. If something is done or configured incorrectly, the work will be nothing. But in general, the client is tested and works.</para>
  /// <code lang="C#">
  /// OAuthManager.RegisterClient
  /// (
  ///   new YahooClient
  ///   (
  ///     "dj0yJmk9Qm1vZ3p2TmtQUm4zJmQ9WVdrOU4wbGlkWGxJTkc4bWNHbzlNQS0tJnM9Y29uc3VtZXJzZWNyZXQmeD0xZQ--", 
  ///     "a55738627652db0acfe464de2d9be13963b0ba1f"
  ///   )
  /// );
  /// </code>
  /// <code lang="VB">
  /// OAuthManager.RegisterClient _
  /// (
  ///   New YahooClient _
  ///   (
  ///     "dj0yJmk9Qm1vZ3p2TmtQUm4zJmQ9WVdrOU4wbGlkWGxJTkc4bWNHbzlNQS0tJnM9Y29uc3VtZXJzZWNyZXQmeD0xZQ--", 
  ///     "a55738627652db0acfe464de2d9be13963b0ba1f"
  ///   )
  /// )
  /// </code>
  /// <para>
  /// For more details, please visit <see href="https://developer.yahoo.com/">Yahoo Developer Network</see>.
  /// </para>
  /// </remarks>
  /// <seealso cref="AmazonClient"/>
  /// <seealso cref="DropboxClient"/>
  /// <seealso cref="FacebookClient"/>
  /// <seealso cref="FoursquareClient"/>
  /// <seealso cref="GitHubClient"/>
  /// <seealso cref="GoogleClient"/>
  /// <seealso cref="LinkedInClient"/>
  /// <seealso cref="LiveClient"/>
  /// <seealso cref="MailRuClient"/>
  /// <seealso cref="OdnoklassnikiClient"/>
  /// <seealso cref="SoundCloudClient"/>
  /// <seealso cref="TwitterClient"/>
  /// <seealso cref="VkontakteClient"/>
  /// <seealso cref="YahooClient"/>
  /// <seealso cref="YandexClient"/>
  public class YahooClient : OAuth2Client
  {

    /// <summary>
    /// Unique provider name: <b>Yahoo</b>.
    /// </summary>
    public override string ProviderName
    {
      get
      {
        return "Yahoo";
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooClient"/>.
    /// </summary>
    /// <param name="clientId">The <b>Consumer Key</b> obtained from the <see href="https://developer.apps.yahoo.com/projects">Yahoo Developer Dashboard</see>.</param>
    /// <param name="clientSecret">The <b>Consumer Secret</b> obtained from the <see href="https://developer.apps.yahoo.com/projects">Yahoo Developer Dashboard</see>.</param>
    public YahooClient(string clientId, string clientSecret) : base
    (
      "https://api.login.yahoo.com/oauth2/request_auth",
      "https://api.login.yahoo.com/oauth2/get_token",
      clientId,
      clientSecret
    ) { }


    /// <summary>
    /// Gets the user details.
    /// </summary>
    public override UserInfo GetUserInfo()
    {
      string url = String.Format("https://social.yahooapis.com/v1/user/{0}/profile?format=json", this.AccessToken["xoauth_yahoo_guid"]);
      
      var result = OAuthUtility.ExecuteRequest
      (
        method:         "GET", 
        endpoint:       url,
        authorization:  String.Format("Bearer {0}", this.AccessToken["access_token"])
      );

      var map = new ApiDataMapping();
      map.Add("guid", "UserId", typeof(string));
      map.Add("givenName", "FirstName");
      map.Add("familyName", "LastName");
      map.Add("nickname", "DisplayName");
      map.Add("profileUrl", "Url");
      map.Add("birthdate", "Birthday", typeof(DateTime), @"MM\/dd\/yyyy");
      map.Add
      (
        "gender", "Sex",
        delegate(object value)
        {
          if (value == null) { return Sex.None; }
          if (value.ToString().Equals("M", StringComparison.OrdinalIgnoreCase))
          {
            return Sex.Male;
          }
          else if (value.ToString().Equals("F", StringComparison.OrdinalIgnoreCase))
          {
            return Sex.Female;
          }
          return Sex.None;
        }
      );
      map.Add
      (
        "image", "Userpic",
        delegate(object value)
        {
          return OAuthUtility.GetDictionaryValueOrNull(value, "imageUrl");
        }
      );
      map.Add
      (
        "phones", "Phone",
        delegate(object value)
        {
          return OAuthUtility.GetDictionaryValueOrNull(value, "number");
        }
      );

      return new UserInfo(result["profile"] as Dictionary<string, object>, map);
    }

    /// <summary>
    /// Gets the access token from the remote server.
    /// </summary>
    protected override void GetAccessToken()
    {
      if (String.IsNullOrEmpty(this.AuthorizationCode))
      {
        throw new ArgumentNullException("AuthorizationCode");
      }

      var parameters = new NameValueCollection
      {
        { "client_id", this.ApplicationId },
        { "client_secret", this.ApplicationSecret },
      };

      if (!String.IsNullOrEmpty(this.ReturnUrl))
      {
        parameters.Add("redirect_uri", this.ReturnUrl);
      }

      parameters.Add("code", this.AuthorizationCode);
      parameters.Add("grant_type", "authorization_code");

      var result = OAuthUtility.ExecuteRequest
      (
        method:         "POST",
        endpoint:       this.AccessTokenUrl,
        parameters:     parameters,
        authorization:  String.Format("Basic {0}", OAuthUtility.ToBase64String("{0}:{1}", this.ApplicationId, this.ApplicationSecret))
      );

      if (result.ContainsKey("error"))
      {
        this.AccessToken = new ErrorResult(result);
      }
      else
      {
        this.AccessToken = new OAuth2AccessToken(result);
      }
    }

  }

}
