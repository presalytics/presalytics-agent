using System.Dynamic;
using System;
using System.Collections.Generic;

namespace test_integration.Setup
{
    public class TestToken
    {
        public static dynamic Get()
        {
            dynamic Token = new ExpandoObject();
            Token.sub = Guid.NewGuid();
            Token.permissions = new [] {"builder", "viewer"};
            ((IDictionary<string, Object>)Token).Add("https://api.presalytics.io/api_user_id", Guid.NewGuid().ToString());
            return (dynamic)Token;
        }
    }
}