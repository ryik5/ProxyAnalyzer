            //разбиваем домен на уровни
            string[] domainLevels = domainName_TB.Text.Trim().Split('.');
            //по шагам пытаемся найти WHOIS-сервер для доменной зоны различного уровня от большей к меньшей
            for (int a = 1; a < domainLevels.Length; a++){
                /*
                 * Если требуется информация по домену test.some-name.ru.com,
                 * то сначала попытаемся найти WHOIS-сервера для some-name.ru.com,
                 * после для ru.com и если всё ещё не найдём, то для com
                */
                string zone = string.Join(".", domainLevels, a, domainLevels.Length - a);
                whoisServers = WhoisService.GetWhoisServers(zone);
                //если нашли WHOIS-сервер, то поиск прекращаем
                if (whoisServers.Count > 0)
                    break;
            }

            if (whoisServers == null || whoisServers.Count == 0)
                result_TB.Text = domainName_TB.Text + "\r\n----------------\r\nНеизвестная доменная зона";
            else{
                result_TB.Text = "";
                foreach (string whoisServer in whoisServers)
                    result_TB.Text += WhoisService.Lookup(whoisServer, domainName_TB.Text);
            }
