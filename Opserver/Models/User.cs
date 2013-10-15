﻿using System;
using System.Security.Principal;
using System.Web.Security;
using StackExchange.Opserver.Models.Security;

namespace StackExchange.Opserver.Models
{
    public class User : IPrincipal
    {
        public IIdentity Identity { get; private set; }

        public string AccountName { get; private set; }
        public bool IsAnonymous { get; private set; }

        public User(IIdentity identity)
        {
            Identity = identity;
            var i = identity as FormsIdentity;
            if (i == null)
            {
                IsAnonymous = true;
                return;
            }

            IsAnonymous = !i.IsAuthenticated;
            if (i.IsAuthenticated)
                AccountName = i.Name;
        }

        public bool IsInRole(string role)
        {
            Roles r;
            return Enum.TryParse(role, out r) && IsInRole(r);
        }

        public bool IsInRole(Roles roles)
        {
            return (Role & roles) != Roles.None || Role.HasFlag(Roles.GlobalAdmin);
        }

        private Roles? _role;
        public Roles? RawRoles { get { return _role; } }
        
        /// <summary>
        /// Returns this user's role on the current site.
        /// </summary>
        public Roles Role
        {
            get
            {
                if (_role == null)
                {
                    if (IsAnonymous)
                    {
                        _role = Roles.Anonymous;
                    }
                    else
                    {
                        var result = Roles.Authenticated;

                        if (Current.Security.IsAdmin) result |= Roles.GlobalAdmin;

                        if (Current.Settings.Dashboard.HasAccess()) result |= Roles.Dashboard;
                        if (Current.Settings.Dashboard.IsAdmin()) result |= Roles.DashboardAdmin | Roles.Dashboard;

                        if (Current.Settings.Exceptions.HasAccess()) result |= Roles.Exceptions;
                        if (Current.Settings.Exceptions.IsAdmin()) result |= Roles.ExceptionsAdmin | Roles.Exceptions;

                        if (Current.Settings.HAProxy.HasAccess()) result |= Roles.HAProxy;
                        if (Current.Settings.HAProxy.IsAdmin()) result |= Roles.HAProxyAdmin | Roles.HAProxy;

                        if (Current.Settings.SQL.HasAccess()) result |= Roles.SQL;
                        if (Current.Settings.SQL.IsAdmin()) result |= Roles.SQLAdmin | Roles.SQL;

                        if (Current.Settings.Elastic.HasAccess()) result |= Roles.Elastic;
                        if (Current.Settings.Elastic.IsAdmin()) result |= Roles.ElasticAdmin | Roles.Elastic;

                        _role = result;
                    }
                }
                return Current.Security.IsInternalIP(Current.RequestIP)
                           ? _role.Value | Roles.InternalRequest
                           : _role.Value;
            }
        }

        public bool IsGlobalAdmin { get { return IsInRole(Roles.GlobalAdmin); } }
        public bool IsExceptionAdmin { get { return IsInRole(Roles.ExceptionsAdmin); } }
        public bool IsHAProxyAdmin { get { return IsInRole(Roles.ExceptionsAdmin); } }
        public bool IsSQLAdmin { get { return IsInRole(Roles.SQLAdmin); } }
    }
}