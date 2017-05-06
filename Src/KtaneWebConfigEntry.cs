﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.Collections;
using RT.Util.ExtensionMethods;
using RT.Util.Json;
using RT.Util.Serialization;

namespace KtaneWeb
{
    sealed class KtaneWebConfigEntry : IEquatable<KtaneWebConfigEntry>
    {
#pragma warning disable 0649 // Field is never assigned to, and will always have its default value

        public string BaseDir = @"D:\Sites\KTANE\Public";
        public string[] DocumentDirs = new[] { "HTML", "PDF" };
        public string[] OriginalDocumentIcons = new[] { "HTML/img/html_manual.png", "HTML/img/pdf_manual.png" };
        public string[] ExtraDocumentIcons = new[] { "HTML/img/html_manual_embellished.png", "HTML/img/pdf_manual_embellished.png" };
        public string ModIconDir = "Icons";

        public string JavaScriptFile;
        public string CssFile;

#pragma warning restore 0649 // Field is never assigned to, and will always have its default value

        [ClassifyNotNull]
        public ListSorted<KtaneModuleInfo> KtaneModules = new ListSorted<KtaneModuleInfo>(CustomComparer<KtaneModuleInfo>.By(mod => mod.SortKey));

        [ClassifyNotNull]
        public HashSet<string> AllowedEditors = new HashSet<string>();

        public JsonList EnumerateSheetUrls(string moduleName, string[] notModuleNames)
        {
            if (moduleName == null)
                throw new ArgumentNullException(nameof(moduleName));

            var list = new List<JsonDict>();
            for (int i = 0; i < DocumentDirs.Length; i++)
            {
                var dirInfo = new DirectoryInfo(Path.Combine(BaseDir, DocumentDirs[i]));
                foreach (var inf in dirInfo.EnumerateFiles($"{moduleName}.*").Select(f => new { File = f, Icon = OriginalDocumentIcons[i] }).Concat(dirInfo.EnumerateFiles($"{moduleName} *").Select(f => new { File = f, Icon = ExtraDocumentIcons[i] })))
                    if (!notModuleNames.Any(inf.File.Name.StartsWith))
                        list.Add(new JsonDict {
                            { "name", $"{Path.GetFileNameWithoutExtension(inf.File.Name)} ({inf.File.Extension.Substring(1).ToUpperInvariant()})" },
                            { "url", $"{DocumentDirs[i]}/{inf.File.Name.UrlEscape()}" },
                            { "icon", inf.Icon }
                        });
            }
            return list.OrderBy(item => item["name"].GetString()).ToJsonList();
        }

        public bool Equals(KtaneWebConfigEntry other)
        {
            return other != null &&
                other.BaseDir == BaseDir &&
                other.DocumentDirs.SequenceEqual(DocumentDirs) &&
                other.OriginalDocumentIcons.SequenceEqual(OriginalDocumentIcons) &&
                other.ExtraDocumentIcons.SequenceEqual(ExtraDocumentIcons) &&
                other.ModIconDir == ModIconDir &&
                other.JavaScriptFile == JavaScriptFile &&
                other.CssFile == CssFile &&
                other.KtaneModules.SequenceEqual(KtaneModules) &&
                other.AllowedEditors.SequenceEqual(AllowedEditors);
        }

        public override int GetHashCode() => Ut.ArrayHash(BaseDir, Ut.ArrayHash(DocumentDirs), Ut.ArrayHash(OriginalDocumentIcons), Ut.ArrayHash(ExtraDocumentIcons),
            ModIconDir, JavaScriptFile, CssFile, Ut.ArrayHash(KtaneModules), Ut.ArrayHash(AllowedEditors));
        public override bool Equals(object obj) => Equals(obj as KtaneWebConfigEntry);
    }
}
