using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrKouk.Web.ERP.Helpers;

public static class ScriptHelper
{
    private const string ScriptKey = "InjectedScripts";

    public static void AddScript(ViewContext context, string script)
    {
        var scripts = context.HttpContext.Items[ScriptKey] as List<string>;
        if (scripts == null)
        {
            scripts = new List<string>();
            context.HttpContext.Items[ScriptKey] = scripts;
        }
        scripts.Add(script);
    }

    public static IHtmlContent RenderScripts(HttpContext httpContext)
    {
        var scripts = httpContext.Items[ScriptKey] as List<string> ?? new();
        return new HtmlString(string.Join(Environment.NewLine, scripts));
    }
}