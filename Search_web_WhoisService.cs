
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WHOIS{
    public static class WhoisService{
        static XmlDocument _serverList = null;

        public static List<string> GetWhoisServers(string domainZone){
            if (_serverList == null){
                _serverList = new XmlDocument();
                //загружаем XML если ранее он не был загружен
                _serverList.Load("whois-server-list.xml");
            }
            List<string> result = new List<string>();
            //определяем функцию для рекурсивной обработки XML
            Action<XmlNodeList> find = null;
            find = new Action<XmlNodeList>((nodes) =>{
                foreach (XmlNode node in nodes)
                    if (node.Name == "domain"){
                        //находим в XML документе интересующую нас зону
                        if (node.Attributes["name"] != null && node.Attributes["name"].Value.ToLower() == domainZone){
                            foreach (XmlNode n in node.ChildNodes)
                                //забираем все адреса серверов, по которым можно получить данные о домене в требуемой зоне
                                if (n.Name == "whoisServer"){
                                    XmlAttribute host = n.Attributes["host"];
                                    if (host != null && host.Value.Length > 0 && !result.Contains(host.Value))
                                        result.Add(host.Value);
                                }
                        }
                        find(node.ChildNodes);
                    }
            });
            find(_serverList["domainList"].ChildNodes);
            return result;
        }

        public static string Lookup(string whoisServer, string domainName){
            try{
                if (string.IsNullOrEmpty(whoisServer) || string.IsNullOrEmpty(domainName))
                    return null;

                //Punycode-конвертер (если требуется)
                Func<string, string> formatDomainName = delegate(string name){
                    return name.ToLower()
                        //если в названии домена есть нелатинские буквы и это не цифры и не точка и не тире,
                        //например, "россия.рф" то сконвертировать имя в XN--H1ALFFA9F.XN--P1AI
                        .Any(v => !"abcdefghijklmnopqrstuvdxyz0123456789.-".Contains(v)) ?
                            new IdnMapping().GetAscii(name) ://вернуть в Punycode
                            name;//вернуть исходный вариант
                };

                StringBuilder result = new StringBuilder();
                result.AppendLine("По данным " + whoisServer + ": ------------------------------------------");
                using (TcpClient tcpClient = new TcpClient()){
                    //открываем соединение с сервером WHOIS
                    tcpClient.Connect(whoisServer.Trim(), 43);
                    byte[] domainQueryBytes = Encoding.ASCII.GetBytes(formatDomainName(domainName) + "\r\n");
                    using (Stream stream = tcpClient.GetStream()){
                        //отправляем запрос на сервер WHOIS
                        stream.Write(domainQueryBytes, 0, domainQueryBytes.Length);
                        //читаем ответ в формате UTF8, так как некоторые национальные домены содержат информацию на местном языке
                        using (StreamReader sr = new StreamReader(tcpClient.GetStream(), Encoding.UTF8)){
                            string row;
                            while ((row = sr.ReadLine()) != null)
                                result.AppendLine(row);
                        }
                    }
                }
                result.AppendLine("---------------------------------------------------------------------\r\n");
                return result.ToString();
            }catch{}
            return "Не удалось получить данные с сервера " + whoisServer;
        }

    }
}
