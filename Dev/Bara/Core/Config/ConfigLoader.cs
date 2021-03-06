﻿using Bara.Abstract.Config;
using Bara.Abstract.Core;
using Bara.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bara.Core.Config
{
    public abstract class ConfigLoader : IConfigLoader
    {
        public abstract void Dispose();
        public abstract BaraMapConfig Load(string filePath, IBaraMapper baraMapper);

        public BaraMapConfig LoadConfig(ConfigStream ConfigStream, IBaraMapper baraMapper)
        {
            using (ConfigStream.Stream)
            {
                XmlSerializer xmlSeralizer = new XmlSerializer(typeof(BaraMapConfig));
                BaraMapConfig config = xmlSeralizer.Deserialize(ConfigStream.Stream) as BaraMapConfig;
                config.Path = ConfigStream.Path;
                config.BaraMapper = baraMapper;
                config.BaraMaps = new List<BaraMap> { };
                return config;
            }
        }

        public BaraMap LoadBaraMap(ConfigStream configStream, BaraMapConfig baraMapConfig)
        {
            using (configStream.Stream)
            {
                var baraMap = new BaraMap
                {
                    BaraMapConfig = baraMapConfig,
                    Path = configStream.Path,
                    Caches=new List<Model.Cache> { },
                    Statements = new List<Statement> { },
                };

                XDocument xdoc = XDocument.Load(configStream.Stream);
                XElement xele = xdoc.Root;
                XNamespace ns = xele.GetDefaultNamespace();
                IEnumerable<XElement> StatementList = xele.Descendants(ns + "Statement");
                baraMap.Scope = (String)xele.Attribute("Scope");

                //Load CacheConfig
                IEnumerable<XElement> CacheList = xele.Descendants(ns+"Cache");
                foreach (var cache in CacheList)
                {
                    //Load XElement
                    var _cache = Bara.Model.Cache.Load(cache);
                    baraMap.Caches.Add(_cache);
                }

                //Load Statements
                foreach (var statementNode in StatementList)
                {
                    var _statement = Statement.Load(statementNode, baraMap);
                    baraMap.Statements.Add(_statement);
                }
                return baraMap;
            }
        }
    }
}
