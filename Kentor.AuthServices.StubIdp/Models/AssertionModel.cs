﻿using Kentor.AuthServices.Saml2P;
using Kentor.AuthServices.WebSso;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IdentityModel.Metadata;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Hosting;

namespace Kentor.AuthServices.StubIdp.Models
{
    public class AssertionModel
    {
        [Required]
        [Display(Name = "Assertion Consumer Service Url")]
        public string AssertionConsumerServiceUrl { get; set; }

        [Display(Name = "Relay State")]
        [StringLength(80)]
        public string RelayState { get; set; }

        [Display(Name = "Subject NameId")]
        [Required]
        public string NameId { get; set; }

        public ICollection<AttributeStatementModel> AttributeStatements { get; set; }

        /// <summary>
        /// Creates a new Assertion model with values from web.config
        /// </summary>
        /// <returns>An <see cref="AssertionModel"/></returns>
        public static AssertionModel CreateFromConfiguration()
        {
            return new AssertionModel
            {
                AssertionConsumerServiceUrl = ConfigurationManager.AppSettings["defaultAcsUrl"],
                NameId = ConfigurationManager.AppSettings["defaultNameId"]
            };
        }

        [Display(Name = "In Response To ID")]
        public string InResponseTo { get; set; }

        public Saml2Response ToSaml2Response()
        {
            var claims =
                new Claim[] { new Claim(ClaimTypes.NameIdentifier, NameId) }
                .Concat((AttributeStatements ?? Enumerable.Empty<AttributeStatementModel>()).Select(att => new Claim(att.Type, att.Value)));
            var identity = new ClaimsIdentity(claims);

            Saml2Id saml2Id = null;
            if (!String.IsNullOrEmpty(InResponseTo))
            {
                saml2Id = new Saml2Id(InResponseTo);
            }

            return new Saml2Response(
                new EntityId(UrlResolver.MetadataUrl.ToString()),
                CertificateHelper.SigningCertificate, new Uri(AssertionConsumerServiceUrl),
                saml2Id, RelayState, identity);
        }

        [Display(Name = "Incoming AuthnRequest")]
        public string AuthnRequestXml { get; set; }

        public Saml2BindingType ResponseBinding { get; set; } = Saml2BindingType.HttpPost;
    }
}