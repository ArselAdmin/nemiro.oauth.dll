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

// If it works, no need to change the code. 
// Just use it! ;-)

namespace Nemiro.OAuth.Clients
{

  /// <summary>
  /// OAuth client for <b>Foursquare</b>.
  /// </summary>
  /// <remarks>
  /// <h1>Register and Configure a Foursquare Application</h1>
  /// <list type="table">
  /// <item>
  /// <term><img src="../img/warning.png" alt="(!)" title="" /></term>
  /// <term>
  /// <b>Web Management Interface may change over time. Applications registration shown below may differ.</b><br />
  /// If the interface is changed, you need to register the application and get <b>Client ID</b> and <b>Client Secret</b>. For web projects, configure <b>return URLs</b>.<br />
  /// If you have any problems with this, please <see href="https://github.com/alekseynemiro/nemiro.oauth.dll/issues">visit issues</see>. If you do not find a solution to your problem, you can <see href="https://github.com/alekseynemiro/nemiro.oauth.dll/issues/new">create a new question</see>.
  /// </term>
  /// </item>
  /// </list>
  /// <para>Open the <b><see href="https://foursquare.com/oauth">Foursquare App</see></b> and <b>Create app</b>.</para>
  /// <para>
  /// In the application settings  you can found <b>Client ID</b> and <b>Client Secret</b>.
  /// Use this for creating an instance of the <see cref="FoursquareClient"/> class.
  /// </para>
  /// <code lang="C#">
  /// OAuthManager.RegisterClient
  /// (
  ///   new FoursquareClient
  ///   (
  ///     "LHYZN1KUXN50L141QCQFNNVOYBGUE3G3FCWFZ3EEZTOZHY5Q", 
  ///     "HWXYFLLSS2IUQ0H4XNCDAZEFZKIU3MZRP5G55TNBDHRPNOQT"
  ///   )
  /// );
  /// </code>
  /// <code lang="VB">
  /// OAuthManager.RegisterClient _
  /// (
  ///   New FoursquareClient _
  ///   (
  ///     "LHYZN1KUXN50L141QCQFNNVOYBGUE3G3FCWFZ3EEZTOZHY5Q", 
  ///     "HWXYFLLSS2IUQ0H4XNCDAZEFZKIU3MZRP5G55TNBDHRPNOQT"
  ///   )
  /// )
  /// </code>
  /// <para>
  /// For more details, please visit <see href="https://developer.foursquare.com/">Foursquare for Developers</see>.
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
  public class FoursquareClient : OAuth2Client
  {

    /// <summary>
    /// Unique provider name: <b>Foursquare</b>.
    /// </summary>
    public override string ProviderName
    {
      get
      {
        return "Foursquare";
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FoursquareClient"/>.
    /// </summary>
    /// <param name="clientId">The <b>Client ID</b> obtained from the <see href="https://foursquare.com/oauth">Foursquare Apps</see>.</param>
    /// <param name="clientSecret">The <b>Client Secret</b> obtained from the <see href="https://foursquare.com/oauth">Foursquare Apps</see>.</param>
    public FoursquareClient(string clientId, string clientSecret) : base
    (
      "https://foursquare.com/oauth2/authenticate",
      "https://foursquare.com/oauth2/access_token", 
      clientId,
      clientSecret
    ) 
    { }
    
    /// <summary>
    /// Gets the user details.
    /// </summary>
    /// <returns>
    /// <para>Returns an instance of the <see cref="UserInfo"/> class, containing information about the user.</para>
    /// </returns>
    public override UserInfo GetUserInfo()
    {
      // https://developer.foursquare.com/docs/users/users

      // query parameters
      var parameters = new NameValueCollection
      { 
        { "oauth_token" , this.AccessToken["access_token"].ToString() },
        { "v", "20141025" }
      };

      // execute the request
      var result = OAuthUtility.ExecuteRequest
      (
        "GET",
        "https://api.foursquare.com/v2/users/self",
        parameters,
        null
      );

      // field mapping
      var map = new ApiDataMapping();
      map.Add("id", "UserId", typeof(string));
      map.Add("firstName", "FirstName");
      map.Add("lastName", "LastName");
      map.Add
      (
        "photo", "Userpic",
        delegate(object value)
        {
          if (value == null || value.GetType() != typeof(Dictionary<string, object>))
          {
            return null;
          }
          var v = value as Dictionary<string, object>;
          if (!v.ContainsKey("prefix") || !v.ContainsKey("suffix")) { return null; }
          return String.Format("{0}300x300{1}", v["prefix"], v["suffix"]);
        }
      );
      map.Add
      (
        "contact", "Email",
        delegate(object value)
        {
          return OAuthUtility.GetDictionaryValueOrNull(value, "email");
        }
      );
      map.Add
      (
        "contact", "Phone",
        delegate(object value)
        {
          return OAuthUtility.GetDictionaryValueOrNull(value, "phone");
        }
      );
      map.Add
      (
        "gender", "Sex",
        delegate(object value)
        {
          if (value.ToString().Equals("male", StringComparison.OrdinalIgnoreCase))
          {
            return Sex.Male;
          }
          else if (value.ToString().Equals("female", StringComparison.OrdinalIgnoreCase))
          {
            return Sex.Female;
          }
          return Sex.None;
        }
      );

      // parse the server response and returns the UserInfo instance
      return new UserInfo(((Dictionary<string, object>)result["response"])["user"] as Dictionary<string, object>, map);
    }

  }

}