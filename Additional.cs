using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyAnalyser
{
    // Класс для построения загoловков PDF iTextSharp - footer and header
    class MyHeaderFooterEvent : iTextSharp.text.pdf.PdfPageEventHelper
    {
        private string _subHeaderText;
        private string _timerText;
        private int _pageNo;

        iTextSharp.text.pdf.BaseFont baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(Environment.ExpandEnvironmentVariables(@"%systemroot%\fonts\Calibri.TTF"), "CP1251", iTextSharp.text.pdf.BaseFont.EMBEDDED);
        iTextSharp.text.pdf.BaseFont boldFont = iTextSharp.text.pdf.BaseFont.CreateFont(Environment.ExpandEnvironmentVariables(@"%systemroot%\fonts\Calibri.TTF"), "CP1251", iTextSharp.text.pdf.BaseFont.EMBEDDED);
        //iTextSharp.text.Font FONT = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 6, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
        //iTextSharp.text.Font FONT = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 6, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);


        public string SubHeaderText
        {
            get { return _subHeaderText; }
            set { _subHeaderText = value; }
        }
        public string TimerText
        {
            get { return _timerText; }
            set { _timerText = value; }
        }
        public int PageNumber
        {
            get { return _pageNo; }
            set { _pageNo = value; }
        }

        public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            iTextSharp.text.Rectangle page = document.PageSize;
            iTextSharp.text.pdf.PdfContentByte canvas = writer.DirectContent;
            iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_LEFT, new iTextSharp.text.Phrase("ProxyAnalyser", new iTextSharp.text.Font(baseFont, 6)), 20, 20, 0);
            iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_LEFT, new iTextSharp.text.Phrase(" ©RYIK 2016-2017", new iTextSharp.text.Font(baseFont, 6)), 510, 20, 0);
            //iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_CENTER, new iTextSharp.text.Phrase(SubHeaderText, new iTextSharp.text.Font(baseFont, 8)), (page.Left + page.Right) / 2, page.Height - document.TopMargin - 5, 0);
            iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_CENTER, new iTextSharp.text.Phrase(SubHeaderText, new iTextSharp.text.Font(baseFont, 8)), (page.Left + page.Right) / 7, page.Height - document.TopMargin / 2, 0);
            iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_CENTER, new iTextSharp.text.Phrase(string.Format("Страница {0}", ++PageNumber), new iTextSharp.text.Font(baseFont, 10)), (page.Right + page.Left) / 2, document.BottomMargin, 0);
            iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_RIGHT, new iTextSharp.text.Phrase(TimerText, new iTextSharp.text.Font(baseFont, 6)), page.Right - 10, page.Height - document.TopMargin / 2, 0);
        }
    }

    //http://metanit.com/sharp/tutorial/15.2.php
    //Классы для  таблиц сводных, помесячных и др.
    struct _StatisticsDirection
    {
        public int _iD { get; set; } //Primary Key
        public string _Direction { get; set; } //category
        public string _Discription { get; set; } //discription of category
        public double _DirectionBytes { get; set; } //GB
        public double _Time { get; set; } //hours
        public string _User { get; set; } //UserLogin
    }

    struct _StatisticsFull
    {
        public int _iD { get; set; } //Primary Key

        public string _Url { get; set; } //URL
        public double _Bytes { get; set; } //MB
        public double _Time { get; set; } //minutes
        public string _Direction { get; set; } //category

        public string _Month { get; set; } //Apr
        public int _Year { get; set; } //Year
        public string _User { get; set; } //UserLogin
    }

    class _MakeIni
    {
        StringBuilder sb = new StringBuilder();
        public void CreateIni()
        {
            sb.AppendLine(@"# ProxyAnalyser.ini");
            sb.AppendLine(@"# Author @RYIK 2016-2018");
            sb.AppendLine(@"# Дата обновления файла:  22.06.2018 23:39:16");
            sb.AppendLine(@"# Start of Configuration");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";Direction.");
            sb.AppendLine(@"# URL по направлениям. Может быть несколько строк с одним направлением. URL Разделять разделять пробелом. ");
            sb.AppendLine(@"# Примеры:");
            sb.AppendLine(@"# xxx = xxx tits");
            sb.AppendLine(@"# xxx = xuy.com");
            sb.AppendLine(@"# microsoft = microsoft.com");
            sb.AppendLine(@"# Direction1 = URL1 URL2");
            sb.AppendLine(@"# Direction1 = URL3");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@"Common = microsoft.com windowsupdate.com microsofttranslator.com msn.com bing.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"# DigitalAds = adfox.ru c8.net.ua dt00.net st00.net .umh.ua adland.ru admaster.net adme.ru admitad.com admixer.net adpro.ua adriver.ru adrock.com.ua adru.net advbroker.ru advert advideo.com.ua alexegarmin.com alpha-alpha.ru avers.ua awalon.com.ua backromy.com banner.kiev.ua banner.ua begun.ru ");
            sb.AppendLine(@"# DigitalAds = bigbordi.com.ua brainberry.ua citysites.com.ua doubleclick.net elephantmedia.com.ua exoclick.com fabika.ru goodadvert.ru google-analytics.com googlesyndication.com holder.com.ua itcg.ua kolitat.com ladycash.ru lux-bn.com.ua luxup.ru marva.ru mediatraffic.com.ua michurin.com.ua mstarproject.com ");
            sb.AppendLine(@"# DigitalAds = openmedia.com.ua outdoor-city.com.ua post.rmbn.ru prime-group.com.ua reklama sellbe.com smi2.net sostav.ua spyboard.net trafmag.com videoclick.ru vidigital.ru vongomedia.ru vy-veska.com.ua yieldmanager.com ");
            sb.AppendLine(@"# DigitalAds = adsoftheworld.com e-promedio.pl images-amazon.com hbr-russia.ru nypl.org adwords.google.com e-stradivarius.net ctfs.ftn.qq.com picdn.net bambus.com.ua egonomik.com direct.yandex.ru nrb-development.com.ua dmonsters.ru goodadvert.ru snbr-stone.com ill.in.ua materials.crasman.fi ");
            sb.AppendLine(@"# DigitalAds = ggpht.com krutilka.net unipdfconverter.com e-ratings.com.ua bongacash.com likondok.com luxup.ru royaladvertising.ua kuruza.ua propellerads.com rontar.com eclipsemc.com dt00.net trafmag.com abcnet-srv1.mpsa.com biturboplus.org blogun.ru uacdn.org mediatraffic.com.ua scene7.com am15.net livesmi.com ");
            sb.AppendLine(@"# DigitalAds = dekoravto.com.ua restyling.in.ua rarenok.biz propellerads.com pix-cdn.org gpm-digital.com spyoutdoor.com gallerymedia.com.ua zassets.com karo.pk mi6.kiev.ua kaltura.com ");
            sb.AppendLine(@"# DigitalAds = syzygy.net skd-druk.com antbeeprint.com ooyala.com mediateas.com brightcove.com marketgid.com recreativ.ru comodoca.com mmr.ua nvjqm.com youshido.com api2.waladon.com adframesrc.com pay-click.ru wambacdn.net goodadvert.ru pay-click.ru .adocean. pix.eu.criteo.net ");
            sb.AppendLine(@"");
            sb.AppendLine(@"# Finders = google. gstatic.com yandex. meta.ua wikimedia. wikimapia.org wikipedia. bing.com rambler. yahoo. aport. .webalta.ru ");
            sb.AppendLine(@"NewsInfoAds = aol.com magnet.kiev.ua mariupol-express.com.ua marsovet.org.ua mgm.com.ua mreporter.ru msn.com yanukovychleaks.org news novaposhta.ua novias.com.ua novostimira. obozrevatel. online.ua otipb.at.ua paper. podrobnosti. polemika.com.ua popmech.ru pravda.com.ua pravmir.ru redtram russianmanitoba.ca segodnya. silauma.ru sinoptik slando.ua slon.ru smartbooka.net smi2.ru sn00.net stakhanov.org.ua supercoolpics.com telegraf.com.ua thawte.com theatlantic.com timedom.com.ua tochka.net tonis.ua translate.ru utg.ua tugraz uaprom.net ubc-corp.com ubr.ua ucdn.com ukrinform.ua unian.net unn.com.ua ustltd.com vesti.ru rbc.ua .ria.ua liga.net 163.com 112.ua pravda.com wunderground.com golos-ameriki.ru p-p.com.ua sdelanounas.in.ua tut.by vesti-ukr.com politeka.net lentainform.com ");
            sb.AppendLine(@"NewsInfoAds = vido.com.ua v-mire.com vremia.in.ua vtbrussia.ru webtrends.com xvatit.com yaskraviy.com zaxid.net zhitomir.info zirki.info znakiua.com korrespondent.net gazeta.ua synoptyc.com.ua bigmir.net censor.net.ua tvi.ua lenta.ru puls.kiev.ua pravo-kiev.com ts.ua fakty.ictv.ua 06239.com.ua kp.ru intv.ua companion.ua forbes.ru 1tv.com.ua kommersant.ua pinchukfund.org .ukr.net mmr.ua news.meta.ua ipress.ua sledstvie-veli.ks.ua lb.ua kompik.if.ua fakty.ua vashmagazin.ua ntv.ru novosti-n.mk.ua gorod.dp.ua 15minut.org aspo.biz vgorode.ua news.mail.ru vesti.ua dumskaya.net odessa-life.od.ua ukrgo.com glavcom.ua zik.ua delo.ua vz.ua 048.ua timer.od.ua m24.ru mr7.ru cinemaciti.kiev.ua ictv.ua politkuhnya.net uainfo.org mk.ru tsn.ua inter.ua uapress.info znaj.ua apostrophe.ua forexpros.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Images = riastatic.com makeagif.com fotocdn.net ");
            sb.AppendLine(@"");
            sb.AppendLine(@"SocialNets = 205.188.22.193 62.41.58.141 62.41.58.87 64.12.30.66 64.12.98.203 64.211.168.47 64.211.168.60 80.150.142.69 91.190.216.23 91.190.216.24 91.190.216.25 antimir.com.ua badoo.com blogger clubs.ya.ru disqus.com facebook fbcdn.net fbsbx.com fdating.com funs.djuice.ua gidepark.ru googleusercontent.com icq. lavra.spb.ru linkedin live.com liveinternet.ru livejournal love.mail.ru love.viagra.co.ua mad-ptah.com mamba.ru mamboo.com mirtesen.ru my.mail.ru mylivepage.ru ning.com onona.ua planeta.rambler.ru plusone.google.com privet.ru qip.ru skype. spasivdim.org.ua topface.com tumblr.com twimg.com twitter userapi.com vk.com vkontakte.ru vk.me plus.google.com vkadre.ru ");
            sb.AppendLine(@"SocialNets = .fbcdn.net odnoklassniki.ru moimir.org loveplanet.ru 24open.ru lovetime.com mylove.ru tourister.ru 217.20.153. 217.20.145. 217.20.157. sender.mobi intercom.io instagram.com presenta.xyz ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Mailers = accounts.google.com torba.com un.net.ua zakladki.ukr.net freemail.ukr.net smartresponder.ru ");
            sb.AppendLine(@"");
            sb.AppendLine(@"FileStores = akamaihd.net akamaized.net datalock.ru .grt01.com .s3.ua 176.37.57.50 199.91.154.33 213.199.179. 2file.net 37.220.161.181 46.165.200.111 46.98.66.174 4chandata.org 4put.ru 62.109.141.165 78.108.178.215 78.108.179.233 78.108.183.128 78.140.145.105 78.140.170.212 78.140.170.236 78.140.170.68 78.140.178.86 78.140.184.146 78.140.184.147 78.140.184.148 78.140.184.150 78.140.184.160 78.140.184.162 78.140.184.169 78.140.190.243 78.140.190.251 89.184.66.165 93.74.35.248 94.198.240.163 94.198.240.164 94.198.240.18 94.198.240.193 94.198.240.203 94.198.240.212 94.198.240.37 94.198.240.56 94.198.240.96 addthis.com adsua.com amazonaws.com cloudfront.net crl.entrust.net depositfiles. dotua.org dropbox e.mail.ru edgecastcdn.net edisk etsystatic.com fastcdn.me fastpic.ru file-cdn.com files.mail.ru fileshare.in.ua filestore.com.ua firepic.org forumimage.ru fotohost.kz freeshareloader.com fsimg.ru godaddy.com ");
            sb.AppendLine(@"FileStores = googleapis.com hotfile.com ifolder. leaseweb.net letitbit. loadup.ru mediaget.com onlinefilefactory.net vividlabz.com podvignaroda.mil.ru imageban.ru imageshack.us jkgbr.com keep4u.ru .mycdn.me hotcloud. .dropmefiles. storage .turbobit. kor.ill.in.ua 50.7.161.18 ");
            sb.AppendLine(@"FileStores = userfiles.me mkpages.epaperflip.com pawidgets.trafficmanager.net piccy.info radikal.ru rapidshare. rghost.ru rusfolder.com savepic.net sdlc-esd.sun.com sendfile.su slickpic.com slil.ru tchkcdn.com tempfile.ru turbobit. uafile.com.ua unibytes.com uploaded.net uploads.ru verisign.com vimeocdn.com yimg.jp zakachali. api2.waladon.com foto.rambler.ru digitalua.com.ua savepic.org isok.ru media.adrcdn.com grt02.com sendimage.me edisk.ukr.net shutterstock.com ferrari-4me.weebo.it img.ria.ua files.namba.net disk.yandex.ru dfiles.ru sendspace.com rapidview.co.uk us.ua photo.torba.com fayloobmennik.net hotdisk.org rackcdn.com tttmoon.com getitbit.net filecdn.to vcdn.biz files.ukr.net jpe.ru toroff.net habrastorage.org join.me .d-cd.net photofile.ru picatom.com leprosorium.com ftp.havaswwkiev.com.ua googledrive.com wetransfer.com yousendit.com auto-media.com.ua 212.90.177.226 studioavtv.com.ua minus.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Shops = versialux.com.ua funforkids.ru .es.com.ua fashion4you.com.ua .hm.com jam.ua .mp.spb.ru mvp.com.ua price.ua 1001bilet.com.ua 4club.com.ua airis.spb.ru aklas.com.ua albion-books.com alkupone.ru ararat.kiev.ua art-oboi.com.ua atbmarket.com avecoffee.ua babystyle.com.ua barin.kiev.ua bauhauz.com.ua begemot.com.ua benuar.com.ua berislav.com.ua biglion biserok bodo.ua bonprix.ua bookshop.ua brasletik.com.ua bt-baby.com.ua cabinet.ua cafeboutique.com.ua camellia. cd-market.com.ua cheboturka.com.ua chicco.com.ua chmall.ru chytayka.com.ua clasno.com.ua cnd-shellac.com.ua daru.kiev.ua dekor-carpet.com.ua dividan.com.ua dogdevik.net ebay ed-mebel.ru e-komora.com.ua e-kvytok.com.ua emozzi.com.ua empik.ua firework.kiev.ua fisher-price.com.ua flagman.kiev.ua flashmeb.com food-ltd.com.ua galereya.com.ua gardini.net.ua gitara.in.ua gobelen.kiev.ua golka.com.ua groupon.com.ua hipp.ua hitime.com.ua hozdom i-drink.com.ua ids-service.com.ua ");
            sb.AppendLine(@"Shops = kidstaff kovroff.com.ua kupikupon.com.ua kupiskidku.com kupisuvenir.com.ua kuponi.com.ua ladyu.com.ua lemage.com.ua violity.kiev.ua viyar.kiev.ua vppmebel.com.ua v-ticket.com.ua vueling.com witt-international.com.ua wizzair.com yakaboo.ua yarema.ua kupinatao.com shket.com.ua gepur.com.ua mehovoy.com metro-group.com termocomplekt.ru zoostar.com.ua fabika.ru rakuten.co.jp igel.com.ua instrumentk.com.ua intertop.ua intimo.com.ua italiavogs.com kickstarter.org tu-tu.ru ufsa.com.ua ukanc.com.ua uniq.ua uplight.com.ua veshalka.com.ua vilonna.com.ua izumsky.com.ua pullandbear.net vintagetrends.com alibaba.com makeup-shop.com.ua stylesalon.com.ua vovabrend.com.ua tarelki.com.ua prom.ua smilefood.od.ua a.alicdn.com aliexpress.com gotoshop.net.ua ");
            sb.AppendLine(@"Shops = mamamia.ua market.tut.ua matraso.com.ua maxicard.ua mebelok.com mebelstyle.net meblium.com.ua megaskidki.com.ua mens-bag.com metro.ua miraton.ua modanadom.com moda-z.com modern.com.ua modnakasta.ua modna-shtora.ua moni.in.ua muzmania.com.ua muztorg.ua mvk-vostok.com.ua myline.com.ua my-office.com.ua my-watch.com.ua narbutas.com oringo.com.ua ozone.ru pafos.kiev.ua pamyatniki.net.ua papirus.com.ua parter.ua petrovka.ua plastics.ua podushka.com.ua pokupon poparada.com.ua posuda prikid.ua prizolov.in.ua promdesign.ua qrticket.in.ua reloading.com.ua rmigroup.ru robotun.com.ua roda.ua rollhouse.com.ua rondell.kiev.ua rukzak.ua samex. secunda.com.ua sewing.kiev.ua shoe-care.com.ua silpo.ua skidka.ua skidochnik.com.ua sn-style.com.ua soundmaster. style.aliunicorn.com sumki-dina.com.ua superdeal.com.ua sushi-anime.com.ua svitstyle.com.ua tanuki.ru tickets.ua tik-tak.ua time-casio.ru tivardo tk-textile.com.ua tripsta.com.ua ");
            sb.AppendLine(@"Shops = zakupka.com zdorovalavka.com.ua .parkflyer.ru trade-city.ua .bag24.com.ua vipbag.com.ua stilago.com.ua ergopack.ua tovaryplus.ru ukrpapir.com.ua faberlic-online.info bazilkandusupov.com ukrzoloto.ua tktimport.com veneto.ua shop.topsecret.com.ua hilt.com.ua plato.ua stiliaga.com.ua braggart.ua mir-maek.ho.ua xstyle.com.ua shopnow.com.ua koketka-online.com evora.ua centrofashion.ru elit-alco.com.ua napitokclub.net urbanstyle.com.ua asos-media.com kanapa.ho.ua neimanmarcus.com taobaocdn.com timeshop.com.ua bestwatch.com.ua v7kupon.com groupon-cdn.ru ricci.com.ua katalogkartin.com uagallery.com.ua fashion-online.com.ua basconi.com mizo.com.ua superdeal.com.ua posudaclub.kiev.ua welfare.ua ua.all.biz hello-kitty.kiev.ua euroenergo.biz banggood.com avia-booking.com e-travels.com.ua nanoprotec.ua vendors.com.ua luxlingerie.net.ua self-collection.com.ua yatego.com ujena.com bershka.net ");
            sb.AppendLine(@"Shops = individ.ua armored.com.ua conte-kids.by plazma.com.ua booklya.com.ua voda.com.ua bilethouse.com.ua chastime.com.ua itsell.com.ua gustosa.com.ua zvek.com.ua vcolec.com.ua parfums.ua sm-michel.com vanilla.kiev.ua obruchalka.com.ua orix-gold.com.ua zappos.com swarovski.com winefood.com.ua a-sky.in.ua mystyle.kiev.ua icaravan.com.ua tally-weijl.com 08.od.ua forus.com.ua topmall.ua 105.com.ua albertokavalli.com.ua avangard-time.ru imperio.kiev.ua belgusto.com.ua spreadshirt.com vipkupon.com.ua vashashuba.com.ua supermaiki.com ralphlauren.com goodwine.ua megavision.ua fatline.com.ua creativemama.com.ua fashionwatches.com.ua vramke.com.ua preta.com.ua ua.centrofashion.com mydnk.com setadecor.ua topshoptv.com.ua multivarka.pro derby.ua filter.ua med-magazin.com.ua moglee.com bee-pharmacy.com meblinovi.kiev.ua baldessarini.com futbolki.dp.ua reglan.com.ua stamps.kiev.ua mfest.com.ua elitebrand.com.ua olx.ua ");
            sb.AppendLine(@"ShopDigital = .bt.kiev.ua .mo.ua .y.ua 5ok.com.ua agsat.com.ua allo.ua alloxa.com antenka.com.ua apple.com aukro.ua avgold.ru cezar.ua citrus.ua city.com.ua comteh.com deshevshe.net.ua e-katalog fotomag.com.ua fotos.ua foxtrot.com.ua goods.marketgid.com hotline hotprice.ua i-m.com.ua itbox. klondayk.com.ua kpiservice.com.ua magazyaka.com.ua megabite.ua metamarket.ua mobilluck.com.ua mobiset.ru mobitrade.ua nadavi.com.ua notus.com.ua pcshop.ua protoria.ua repka.ua roks.com.ua rozetka satmaste sokol. sotmarket.ru strobist.ua stylus.com.ua technoportal.ua tehnohata.ua torg.alkar.net ukrshops.com.ua vcene.ua avic.com.ua technopolis.com.ua bosch-home.com.ua slinex.kiev.ua stockmobile.ua intermobil.com.ua fotos.com.ua comfy.ua foxmart.ua shop-gsm.net nofelet.in.ua siemens-home.com.ua ");
            sb.AppendLine(@"ShopBuild = .nl.ua 1giper.com.ua accbud.ua agromat.ua altherm.com.ua aney.com.ua bau. bioplast.ua bitovki.kiev.ua bprice.ua brille.ua dizajio.kiev.ua document.ua dokamin.ru dvernik.com.ua ekodveri.in.ua ekonom-remont.com.ua ibud.ua ideidetsploshad.info instrument.in.ua kamni-market.com keramida.com.ua konkurs.ru krainamaystriv.com lampa.kiev.ua liko-holding.com.ua muratordom.com.ua novalinia.com.ua okna.ua proekty.ua promobud.ua proxima.com.ua rabotnik.kiev.ua spectr.kiev.ua stroimdom.com.ua stroymart.com.ua tehnikaokna.ru truba.ua tvoydom.kiev.ua viknadveri.com zaglushka.ru termocom.ru bul-market.com.ua 3dklad.com beton.kovalska.com knauf.ru xn--80a1agg3a.com.ua keramdev.com.ua kupiplitku.com.ua germes-studio.kiev.ua metall-ks.com.ua 3208.ru hunterdouglas.com san-tehnika.com.ua feeder.kiev.ua ekodom.net.ua akm.kiev.ua kamelotstone.ua infohome.com.ua pufic.com.ua rollstroy.narod.ru autonomenergo.com.ua lesprom.kiev.ua perestroika.com.ua praktiker.ua luminaelit.com.ua gunter-hauer.ua mebli-zakaz.kiev.ua ");
            sb.AppendLine(@"ShopBuild = epicentrik.info santehtop.com.ua ceramica.ks.ua maxus.com.ua infohome.com.ua dveri-pol.com.ua zametkielectrika.ru e-1.com.ua balon.kiev.ua voltweld.com promsvarka.com domsvarki.lg.ua in-green.com.ua ");
            sb.AppendLine(@"ShopRieltor = lipinka.com.ua .est.ua .fn.ua .lun.ua 100realty.ua 3doma.ua address.ua appartament.kiev.ua bfontanov.com.ua blagovist.ua chayka.org.ua comforttown.com.ua concord.in.ua country.ua dobovo.com dom.ria.ua dom2000.com dom9.kiev.ua domik.net domproekt.kiev.ua dom-z.com.ua eastbooking.ua elitgrup.com.ua estater.biz etag.com.ua friendsplace.ru home-poster.net hotel kvartiravkieve.com kvartorg.com mdigroup.com.ua megamakler.com.ua meget.kiev.ua miete.com.ua mirkvartir.ua mistechko.com.ua most-city.com novakvartira.com.ua ozimka.com parklane.ua promap.ua prostodom.ua realt rieltor.ua v-irpen.com vkvartir zhitlo.in.ua kanzas.ua prestigehall.com.ua novbud.com.ua richtown.com.ua perlina-kiev.com.ua evrodim.com zeleniykvartal.com.ua bgm.kiev.ua zirka-dnipra.com.ua fn.ua kadorrgroup.com cheremushki.od.ua lun.ua dbk4.com.ua zhk.org.ua 7sky.od.ua levitana.com.ua novostroy.od.ua prazhsky.com.ua kmb-sale.com capital.ua panovision.com.ua mariinsky.com.ua cottage.ru zolotoybereg.com doba.ua oneday.ua ");
            sb.AppendLine(@"ShopRieltor = lunnovostroyki. vnovostroike.com.ua quote-spy.com l-kvartal.com.ua kulumok.kiev.ua b-l.org.ua club-bl.kiev.ua");
            sb.AppendLine(@"ShopBoutiq = z95.ru antoniobiaggi.com.ua b-1.ua bicotone.com.ua brocard.ua butik.ru carlopazolini.com chanel.com cop-copine.com dioriss.com.ua enna-levoni.com etam.com fashionavenue.com.ua gold.ua incanto.ru kuz.ua lanett.ua leboutique.com lediamant.com.ua multi-butik.com red.ua unona.ua zapatos.com.ua zara.net mango.com hm.com daniel.kiev.ua victoriassecret.com topsecret.ua helen-marlen.com pierrecardin-ukraine.com joma.com.ua wittchen leboutique deezee.pl issaplus.r.worldssl.net flashsale.chia.ua ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Banks = rbc.ru finance.ua vab.ua adspynet.com aval.ua bank finline.com.ua fuib.com kruss.kiev.ua nadra.com.ua portmone privat24.ua pumb.ua unicredit 24nonstop.com.ua rsb.ua usstandart.com.ua minfin.com.ua quote-spy.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Profiles = asmap.org.ua 4konverta.com balance.ua buhgalter capitaltimes.com.ua ipk-dszu.kiev.ua kurs.com.ua ligazakon.ua nibu.factor.ua vobu.biz vobu.com.ua ua-auto.com.ua krbizn.com mtsbu.kiev.ua udai.kiev.ua ivc.in.ua otomoto.pl.ua master-d.com.ua ligazakon.net ");
            sb.AppendLine(@"Profiles = smarttender.biz ");
            sb.AppendLine(@"");
            sb.AppendLine(@"HelthSportBeauty = .nba.com 11na11.com 5el.com.ua amrita-ua.pp.ua avon basket.com.ua beintrend.ua championat.com chernomorets dress-code.com.ua dynamo elle.ru fashiontime.ru feelgood.ua football gorodokboxing.com inessa-salon.com.ua jlady.ru kr-zdorovia.com.ua lidiko.com.ua london2012.com lumenis.com.ua makeup.com.ua manutd marykay master-hairstyles.ru master-pletenij.ru median.kiev.ua m-kay.kiev.ua nevrologia.far.ru ngenix.net omorfia.ru oriflame poozico.com rubasket.com sbnation.com shidnycia.com snowboarding. sport synevo.ua terrasport.ua terrikon.com veliki.com.ua veloonline.com velostyle.com.ua ");
            sb.AppendLine(@"HelthSportBeauty = veritas.in.ua wella.com .jv.ru ya-modnaya.ru yves-rocher.ua zefir.ua bet365.com luxoptica.ua ecolab.kiev.ua glossary.ua footboom.com championat.net footclub.com.ua etgdta.com anastasia.net fcdnipro.ua allboxing.ru kiki.sumy.ua gooool.org medsovet.info marathonbet.com extremstyle.ua parimatch.com s5o.ru williamhill.com futbik24.com fc-anji.ru danabol.com.ua cosmopolitan.ru cosmo.ru kosmo.ua ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Ittech = skachatbesplatnyeprogrammy.ru home-soft.com.ua ixbt.com lg.com 1c-bitrix-cdn.ru 3dmir.ru afo.kiev.ua artlebedev.ru china-review.com.ua chip.ua exler.ru gagadget.com htc.com ibm.com iphones.ru iptelecom.net.ua kyivstar.ua lanzone.info lifehacker.ru lingvo.ua mobile-review.com mts.com.ua multitran.ru online.dynamics.com orfo.ru romka.eu samsung.com smartphone.ua softstudio.ru sql.ru tehnoarhiv.ru volia.com esetnod32.ru free-pdf-tools.ru ho.ua jimdo.com macromedia.com mcafee.com adobe mob.ua mozilla opera.com.ua oracle.com paint-net.ru templatemonster.com true.nl download.cdn.mozilla.net autodevel.com bemobile.ua samsung.brawnconsulting.com itc.ua life.ua download.adobe.com qtrax.com wordpress.com wix.com extrimdownloadmanager.com ua.uar.net sipnd.com android-plus.ru android.wildmob.ru android-app.ru svyaznoy.ru mob.ua weebly.com inkfrog.com keddr.com microsoft.com autodesk.com mobile-review.com smartphone.ua itcg.ua samsung.com jimdo.com oodrive.com neulion.com pgp.com ");
            sb.AppendLine(@"Ittech = top-android.org get4mobile.net 4pda.ru photoshop-master.org for-foto.ru eltel.net delfi.ua luxhard.com intertelecom.ua docspal.com 77.88.210.226 logitech-viva.navisite.net speedtest.hatanet.com.ua vsassets.io githubusercontent.com visualstudio.com vo.msecnd.net python.org redhat.com windowsupdate.com update.microsoft.com");
            sb.AppendLine(@"");
            sb.AppendLine(@"LookForAJob = hh.ru hh.ua job rabota trud work");
            sb.AppendLine(@"");
            sb.AppendLine(@"VideoTV = fbvkcdn.com .kinokrad.net .ovva.tv .fs.ua .vtm.be ytimg.com .zerx.ru 109.68.40.68 109.70.232.147 13-e.ru 173.44.34.108 173.44.34.109 194.190.77.133 194.190.77.177 1tv.ru 24tv 3gpfilm.net 78.108.178.203 79.142.100.23 79.142.100.32 91.197.128.34 allserials.tv autopark.tv cdn.ua cochrane.wimp.com dailymotion.com data.intv.ua dmcdn.net flv.bigmir.net fx-film.com.ua good-zona.ru kino kwcdn.kz lilotv.com livetv lovi.tv magnolia-tv.com watch.online.ua megalife.com.ua megastar.in.ua megogo.net mggcdn.net moova.ru movie my-hit. myvi. ntvplus.ru online-24-7.ru openfile.ru play.ukr.net pulta.net rovenkismi.com.ua rutube.ru scifi-tv.ru serialsonline.net smotri.com stopnegoni.ru streamcdn.eu .shtorm.com gidonline youtube.com .twitch.tv bonus-tv.ru ");
            sb.AppendLine(@"VideoTV = media video khabar.kz moonwalk. testlivestream.rfn.ru thespace.org tikilive.com tours-tv.com turner.com tushkan.net tvigle.ru tvzavr.ru ujena.tv ustream.vo.llnwd.net vd-tv.ru videa.hu video vimeo.com vimple.ru vzale.tv webtv.moldtelecom.md whitecdn.org itv.com youtube.com freeetv.com justin.tv aliez.tv media.trkua.tv neulion.net filmix.net media.ntv.ru mover.uz tfilm.tv ovg.cc films-online.su veterok.tv novatv.bg kewego.com multfilmi.at.ua kiniska.com minizal.net spruto.tv liveleak.com clipsonline.org.ua pbh2.com damiti.ru tvbest.net pteachka.ru ustream.tv divan.tv ukrlife.tv bambuser.com portall.tv media.stb.ua megogo.net thesame.tv vidyomani.com planeta-online.tv kintavr.ru kinogo. .kaban.tv Ex-fs.net ivi.ru baskino 37.220.36.40 rutube.ru ");
            sb.AppendLine(@"VideoTV = 91.234.34.154 91.234.34.136 pdbcdn.co hlsvod.rambler.eaglecdn.com 31.28.163.146 .kaltura.com 185.38.12.41 .stb.ua apollostream.xyz lanet.tv 50.7.128.107 ovva.tv 37.220.39.62 185.38.12.50 185.38.12.48 1internet.tv ttvnw.net streamer ollcdn.net ");
            sb.AppendLine(@"");
            sb.AppendLine(@"GPSCar = 194.247.12.35 200stran.ru aerosvit.ua airarabia.com dnepr-oblast.com.ua gunsel.com.ua istrim.com mapia.ua maps openstreetmap.org visicom navitel.su delivery-auto.com flyuia.com map.meta.ua");
            sb.AppendLine(@"");
            sb.AppendLine(@"AutoClubs = .okyami.net .rst.ua .rul.ua .vodiy.kiev.ua astra-club.org.ua autocentre.ua autoconsulting.com.ua autoportal.ua autoreview.ru avtopoligon.info c3-picasso.ru cars.ru drag402.com drive.ru drive2.ru ducati.kontain.com forum.2108.kiev.ua mad4wheels.com manualedereparatie.info mini.ua mitsubishi-club.org moto oktja.ru privat-auto.info sti-club.su topgearrussia.ru topgir. turbo.ua uavto. vladislav.uа vvm-auto.ru zr.ru kia-club.com.ua offroadclub.ru ukraine-trophy.com auto.mail.ru 3dgarage.ru autolines.org.ua help-on-way.ru forum.autoua.net gazel-club.com.ua e30club.ru a2goos.com bodybeat.ru reviews.drom.ru ");
            sb.AppendLine(@"AutoClubs = forum.vodila.net skoda-club.org.ua autowp.ru autobild.by nissan-club.org j-cars.org j-cars.in.ua kostructor.altervista.org aveoclub.info hexagon.narod.ru gazellnext.ru autoevolution.com indianautosblog.com auto.mail.ru retro-avtomobili.net autoplus.su cfts.org.ua ujena.com ua-auto.com.ua youcar.com.ua auto.ria.com auto.mail.ru automps.ru gaz-club.com.ua oldfordclub.net getcar.ua ");
            sb.AppendLine(@"Cars = .lada. nissan-single.com.ua .ferrari. .infiniti. .lu.com.ua .uaz.ru ais.com.ua ais-avto.com.ua ais-market.com.ua .skoda-auto. alfaromeo-ukraine. americanfleet.com.ua atlant-m.in.ua audi auto-planeta.com.ua avtobazar.ua avtoport-kiev.com.ua avtosojuz.ua awt.com.ua bentleyconfigurator.com bmw. cadillac.com chery.net.ua chevrolet citroen. dodge.com.ua ducati-russia.ru euroavto.in fiat ford.com gaz. geely honda hyundai infinitiusa.com infocar. jaguar. kia. lacetti.com.ua lancer.com.ua landrover. lardi-trans.com lexus. maserati.com.ua mazda. mercedes niko.ua niko-ukraine. nissan.eu nissan.goloseevsky.com nissan-vidi. opelukraine. oskar.odessa.ua autogidas.lt ");
            sb.AppendLine(@"Cars = pickup-center.ru planetavto.com.ua porsche. praga-auto.com.ua renault subaru-vidi.com.ua sy. toyota uavto.kiev.ua uaz4x4 vidi-automarket.com.ua volkswagen. winner.ua winnerauto.ua autoutro.ru abw.by ukravto.ua faw.com.ua rstcars.com citroen-center.com.ua ford.ua byd.ua mg.co.uk greatwall-ukraine.com bogdanauto.com.ua nissan.ua landrover-vidi.com.ua eurocar.com.ua zaz.ua msk.obuhov.ru new.skoda-auto.com baz.ua avtek.ua autoline.com.ua cadillac-ais.com automir.com.ua infiniti-vidi.com.ua nissan-moscow.ru usedauto.com.ua autozaz.kiev.ua kievskoda.com ais-kiev-dnepr.com.ua gazlux.com gazgroup.ru avtobazar-ukraine.com.ua polycar.com.ua carsontheweb.com ");
            sb.AppendLine(@"Cars = omega-auto.biz bulavka.ua autobazar.od.ua inter-auto.com.ua otomoto.pl avtorinok.ru autobum.in.ua jeep.ua avtopoisk.ua rst.ua sauto.cz suchen.mobile.de cars.auto.ru ssangyong.ru sweet-auto.com.ua nextcar.ua sollers-auto.com newcars.ua m1.ua.f6m.fr fordodessa.com ford-vidi.com.ua edem-auto.com.ua orient-uaz.ru daihatsu-dias.com.ua kia-kiev.com.ua paritet.com.ua autocredit.com.ua ssangyong-irbis.ru autopark.od.ua m1.ru.f6m.fr vidi-autocity.com avtosale.ua volkswagen-rivne.com aispolis.com.ua bus.ru infiniti-lab.com.ua gm-avtovaz.ru atlant-m.spb.ru scania.ua ffclub.ru mazdadb.com autosite.com.ua peugeot suzuki. tivoli. subaru.ua autotrade.com.ua vis.iaai.com");
            sb.AppendLine(@"AutoLogistika = estafeta.org avtologistika.com 1move.com auto-partner.net spincar.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Parts = vsedlyaavto.kiev.ua avto-diski.com.ua autokovri.com.ua shop.omega-auto.biz .mao.in.ua 3dtuning.ru ais-sp.com.ua autoklad.ua avtoplaneta.com.ua avtozvuk.ua axxa.com.ua baltkam.ru brcgasequipment.ua car-care.com.ua castrol.com chiptuner.ru elcats.ru elit.ua elit-tyres.com.ua vsedoavto.com.ua avtomaler-plus.com.ua b2b.ad.ua ");
            sb.AppendLine(@"Parts = erisin.com exist.ru exist.ua ford-chrom.com.ua gaz-car.ru injapan.ru interlight.biz ip-auto.com.ua japancats led-svet-drl.com losk.ua mannol market.autoua.net masterniva.ru mrcap.com.ua nashashina.com.ua neoriginal.ru polarisind.com radial.com.ua razborki.com shell.com teamparts.ru gazdetal54.ru r-avto.kiev.ua 130.com.ua ");
            sb.AppendLine(@"Parts = tuning-market.od.ua tyretrader.com.ua unit-9.ru vse-o-pokryshkah.ru wheelhunter.com.ua zapadpribor.com zavoli.com.ua sgauto.com.ua catalog.autotechnics.ua avtoparts.com.ua city-auto.com.ua agrosoyuz.com am-servis76.ru 412345.ru uaz-upi.com avtoall.ru auto-sklad.com zapchasti.ria.ua autoprofi73.ru cartuning.in.ua ");
            sb.AppendLine(@"Parts = sherpa-auto.ru point.autoua.net daihatsu.at.ua:rezina.cc auto-light.com.ua avtoradosti.com.ua belcard-grodno.com autodealer.ru konsulavto.ru luxshina.ua shyp-shyna.com.ua bus-comfort.com.ua autoplaz.com.ua pereoborudovanie.com.ua automillenium.com.ua automaidan.com.ua obhivka.com autostyle.zt.ua detal-komplekt.ru ");
            sb.AppendLine(@"Parts = avtobox.com.ua luxsto.com.ua dio.kiev.ua china-shop1.com rdrom.ru carid.com dekoravto.com.ua tuninga.com.ua carmanauto.ru dekoravto.com.ua restyling.in.ua intercars.eu rezina.cc vladislav.ua opletka.net avtika.ru autogas.in.ua soundplanet.com.ua fordfocus.com.ua pixtinauto.ru vazinj.com.ua kolesiko.ua ");
            sb.AppendLine(@"Parts = azvuk.ua kingauto.com.ua premiorri.com aksavto.com.ua emgrand-shop.com rezina13.com.ua kolpak.com.ua autoshini.com shinaplus.com kayaba.com.ua shinservice.ru avtocomfort.com.ua all4cars.com.ua avto.pro market.ria.ua autoscan.com.ua electroshemi.ru ultrastar.ru garazh.com.ua gazok.in.ua razborkabmw-e39.ru ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Teach = fpk.in.ua greenforest.com.ua krok.edu.ua yappi.com.ua lvduvs.edu.ua englisher.com.ua classroom.com.ua languagefree.narod.ru mti.edu.ru window.edu.ru kname.edu.ua gai.ua intuit.ru kneu.edu.ua academic.ru .edu.ua");
            sb.AppendLine(@"");
            sb.AppendLine(@"MusicRadioonline = .galaradio.com .zf.fm 101.ru 109.120.141.174 109.120.141.181 109.234.154.100 109.234.154.119 109.234.154.194 109.234.154.29 109.234.154.30 188.75.223.31 188.93.17.187 188.93.19.234 195.66.153.17 195.95.206.17 195.95.206.214 212.115.229.83 212.26.129.2 212.26.129.222 212.26.146.47 217.171.15.155 217.20.164.163 46.4.98.119 5.79.69.115 62.80.190.246 77.47.134.32 78.159.122.138 79.98.143.194 83.142.232.246 91.201.37.43 91.202.73.76 91.214.237.247 91.214.237.248 91.220.157.3 92.241.191.100 95.81.162.158 akadostream.ru bandcamp.com batzbatz.com clubomba.com europaplus.ua flypage.ru fm.odtrk.km.ua froster.org get-tune.net ");
            sb.AppendLine(@"MusicRadioonline = glob.radiogroup.com.ua globaltranceinvasion.com hitfm. hitru.ru ipfm.net iplayer.fm kissfm.ua lux.fm media.brg.ua media.fregat.com megalyrics. miloman.net molode.com.ua moskva.fm music musvid.net muzebra.com muznarod.net muzofon.com myzuka. ololo.fm optima.fm podfm.ru radio retro.ua rferl.org ringon.ru rorg.zf.fm rpfm.ru setmedia.ru sky.fm snimi.tv soundcloud.com stream.kissfm.ua stream-1.k26.ru tavrmedia.ua thankyou.ru uhradio.com.ua uplink.duplexfx.com zaycev.net sc-atr.1.fm icecastlv.luxnet.ua loungefm.com.ua mixupload.com mixcloud.com muzofond.org zf.fm mp3ton.info mp3xa.pw ");
            sb.AppendLine(@"");
            sb.AppendLine(@"EBooksMagazine = e-reading.org.ua flibusta.net issuu.com lib.ru phoenixcenter.com.ua cbs3vao.narod.ru bookclub.ua");
            sb.AppendLine(@"");
            sb.AppendLine(@"Totalizatos = maxiforex.ru fox-manager.com.ua masterforex-v.org tradernet.ru mavrodi marathonbet.com betcityru.com anyoption.com vo3tok.biz vostok3.com criteo.net superbinary.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Funny = .irc.lv .lah.ru .msl.ua .pnz.ru .uku.com.ua .yimg.com 15rokiv.novy.tv 1zoom.ru 2bobra.com.ua 2k.ua 3karasya.com.ua 4tyres.ua 78.com.ua 99px.ru admiralclub.com.ua adsence.kiev.ua aeroboat.ru afisha.mail.ru akamaihd akkord-tour.com.ua alick666.kiev.ua allelitepass.com allwatch.kiev.ua altantour.com anekdotov.net anextour.com antclub.ru antratsit.net aquafanat.com.ua aquafisher.org.ua aquapark.bg artek.ua artleo.com arutoronto.co.uk asianculture.ru autoprazdnik.ru autowalls.ru avan-ti.net avmc.com.ua avsim.su babai-family.com.ua babyplan.ru postnext.com std3.ru bit.ua vimka.ru spaces.ru spac.me ");
            sb.AppendLine(@"Funny = bastet.in.ua batona.net bayka.info bazi-otdiha.com.ua behance.net bemarry.com.ua berloga.net bestin.ua bestpozitiv.ru best-wedding.com.ua betradar.com bianca-lux.ru bigpicture. bilshe.com bingo.ua bitgravity.com blin.com.ua blockbuster. blogspot.com bobx.com booking.com boranmare.com bts.aero bugaga.ru bukovel.com buro247.ru butterfly.vn.ua buzzfed.com byaki.net canarsky-forum.ru cardsgif.ru car-ups.com channelingstudio.ru chevy-rezzo.narod.ru chistueprudi.ru cinemagraphs.com cityclub.kiev.ua cityfrog.com.ua clever-eyes.com.ua cmexota. stereoplaza.com.ua forum. tutkryto.su gorod.cn.ua userfiles.me ");
            sb.AppendLine(@"Funny = dakotapub.com darina.tv dedmoroz.ru deluxesound.com.ua demotivator deti.mail.ru dipserv.com.ua dirty dnepr.com docker.com.ua dofiga do-gazdy.com.ua dogcat.com.ua dominospizza. domskidok.com doodoo.ru dormstormer.com doroga.ua doseng.org dragobrat-go.com dreamtown.ua dsa-travel.com durdom.in.ua dusia.telekritika.ua dwor-rychwald.pl dyachenko.kiev.ua dyvosvit.ua edimdoma.ru effectfree.ru elementdance.ru elstile.ru esenin.kiev.ua esquire.ru eva.ru evilnight.ru fanfabrika.novy.tv fashion-mix.ru favbet.com feerie.com.ua films-iphone.com fionatravel.com.ua fishing fishki flickr.com superfiles.me ");
            sb.AppendLine(@"Funny = fotochumak.com fotofilmi.ru fotomania.in.ua fototelegraf.ru fotozefir.com.ua fresher.ru funik.ru funny garage21.com.ua gardena.com garriphotoman.pp.ua gartourkonkurs.net geometria.ru gifzona giphy.com gloss.ua goodfon.ru gorets-media.ru gorockop.ru gport.com.ua gradient.cx gradiva.com.ua grandmaideas.com graniart.ru graphics.in.ua gravure-idols.com groupon. gut.ru havana-club.com hawaii-kirillovka.com hd.at.ua hero2012.ru histoiredeshalfs.com hobbydelux.com hohota. horo.mail.ru horoscope hottours.in.ua husky.co.ua hwb.com.ua ibigdan.com ibrovary.com ifun.ru il-patio.com.ua menunedeli.ru thisispivbar.ua ");
            sb.AppendLine(@"Funny = jino.ru jongoo.net joyreactor.cc jphip.com kaifolog.ru kalinka-malinka.com.ua kamelek.com kanzas.ua karaoke.ru karavan.com.ua karpela.com katran-club.com.ua katysha.com.ua kirillovka.su klopp.ru klouny.kiev.ua klukva.org kolesogizni.com kolyan.net kontinent-card.com.ua korchma.kiev.ua korefun.net koroli.kiev.ua korsun.ic.ck.ua korzik.net kotomatrix.ru krabov.net kraina-ua.com kuda.com.ua kuda-ugodno.ua kundalini.com.ua kvitochka.kiev.ua kyxarka.ru leopark.ua lider-bk.com.ua lifeglobe.net look.com.ua lookatme.ru lostworld.com.ua lottery.com.ua loviskidki.com.ua luckyfisher.com.ua ochevidets ");
            sb.AppendLine(@"Funny = luxlux.net luxtv.ua lvivske.com maestro-travel.com.ua mafia.ua mainpeople.ua makuha.ru malva-tour.com.ua mamajeva-sloboda.ua matriarchat.ru mcdonalds.ua mediablender.com.ua mediananny.com memorial.kiev.ua menu.ru migalki.net miph.info mir-animasiya.ru mir-idei.com.ua mirprazdnika.kiev.ua mirvkartinkah.ru mkpages.epageview.com mnogo-idei.com modelist-konstruktor.com monk.com.ua moreleto.com.ua muzey-factov.ru myfishka.com nairi.com.ua nash.com.ua nasha-karta.ua nashaplaneta.net nashpilkah.com.ua nastol.com.ua nataliakabliuk.com nethouse.ua netlore.ru nevozmozhnogo.net ochepyatki.ru cameralabs.org ");
            sb.AppendLine(@"Funny = ngoboi.ru nibler.ru nice-places.com nightparty.ru nocookie.net nudistam.com oboffsem.ru oceanplaza.com.ua ochi.com.ua odessaguide.net ohoter.ru olivertwist.com.ua orakul. originaloff.com.ua orion-intour.com osinka.ru otpusk.com outshoot.ru packpacku.net panoramio.com parkkyivrus.com partsukraine.com.ua passion.ru pattayaphotoguide.com pegast.com.ua pepe.com photo. photoe.kiev.ua photovolkov.com.ua pikabu.ru pikch.ru pipec.ru pirojok.net pitchforkmedia.com pivarium.com.ua pizza. pizza33.com.ua pizza-celentano.kiev.ua playcast.ru poetryclub.com.ua porjat porter.com.ua sweetbook.net chocoapp.ru stranamam.ru ");
            sb.AppendLine(@"Funny = premierworld.com.sg pricheska-kiev.com.ua prikol princessyachts.com prjadko. prochan.com coraltravel creative.su cruze-club.com.ua crystalhall.com.ua cveti.ucoz.ua d3.ru incz.com.ua io.ua irecommend.ru italia.com.ua ittour.com.ua izum.ua jazz.koktebel.info jetsetter.ua versal-online.com.ua vetton.ru vinbazar.com vip.vn.ua virtual.ua vishivay.ru visualization.com.ua bacchusclass.com baginya.org banisauni.com.ua conviva.com cool-birthday.com copypast.ru collie-merrybrook.com studia.kiev.ua nezabarom.ua fotki imgur.com pozdravlenye.com verdiktor.net lurkmore ochepyatki.ru slivki24.club ltu.org.ua ");
            sb.AppendLine(@"Funny = puzatahata.com.ua raduga-club.org raffaello.net.ua ragu.li raskraska.com re-actor.net redbull.com redigo.ru redtubefiles.com relax restaurant-esenin.ru reston.com.ua restoran-stop.com.ua rodynnefoto.com.ua rolandus.org route66.com.ua roxyclub.kiev.ua rtamada.kiev.ua rulez-t.info rusforum.ca rybalka rybinsk20.narod.ru saeco.de sastattoo.com scalemodels.ru schastie.kiev.ua serebro-rmb.com sezon-rybalki.com.ua shopaholic.kiev.ua shtormovoe.crimea.ua skeletov.net skybar.ua snasti.com.ua spankwire.phncdn.com spletnik. starer.ru starlife.com.ua starlightmedia.ua nevsedoma.com.ua anwap.org shutterstock.com ");
            sb.AppendLine(@"Funny = studio37th.com studio-moderna.com sunny7.ua surfingbird.ru sushi-nadom.com.ua sushiya.ua tarantino.com.ua tarhankut.ucoz.ua tastesgood.ua tattoomakers.com.ua teleblondinka.com teplitsacafe.com teztour theadventuresofteamhiemstra.com themeparkreview.com the-village.ru time2eat.com.ua today.kiev.ua toget.ru tophotels tourpalata.org.ua travel.ru trinixy tripadvisor.ru trostyan-rezort.com.ua tunersandmodels.com turbina.ru turne.com.ua turpravda.com turtess.com tury.ru ua.igru-film.net ucoin.net uoor.com.ua urod.ru uti-puti.com.ua vadim-grinberg.com vasi.net vasilkov.info vashapanda.ru trofey.net anje.com.ua ");
            sb.AppendLine(@"Funny = vitalstorage.info viva.ua voboyah.com vodka-bar.com.ua voffka.com vogue.ru vokrug.tv vokrugsveta.com votrube.ru voyage.kiev.ua vsyako-razno.ru wallpapermania.eu wallpapers watch.ru webpark.ru wedlife.ru wetravelin.com wizardcamp.com.ua woman.ru wooms.ru xameleon.club300.com.ua xa-xa.org xkc.com.ua xn--80aqafcrtq.cc ya1.ru yaicom.ru yaki.com.ua yapfiles yaplakal yaremcha.com.ua zagony.ru zapilili.ru zazuzoom.com.ua z-d.com.ua ziza.ru zooclub.com.ua zoo-flo.com zooforum.ru woman.ua 1001mem.ru ruchess.ru gfycat.com fanat.ru yarovikov.ru vengrija.com.ua vbios.com img.com.ua bogolvar.com.ua 1ua.photos fotoaz.com.ua ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Porno = sxgirls.net sweet-girl.su sex3.com putana.cz lurtik.net babestar.ru myero.su erotic-foto.net titya.ru erodomen.ru pr.nsimg.net youngincest.net jjgirls.com familyfuck.org mofos.com 18firstsex.com ybanda.com yobt.com zhestkoeporevo.net porn1xa.net foto-zhenshin.ru kordonivkakino.net 37.72.170.58 overthumbs.com poimel.me ero-pixe.com fap-foto.com ero-pixes.com .xvideos.com .adult poxot.net poxotcdn1.ru ");
            sb.AppendLine(@"Porno = mofos.com .pinkrod.com .rk.com updatetube.com 18kitties.com 18teencore.net 21sextury.com 4tube.com 78.140.136.196 78.140.136.197 78.140.136.198 78.140.181.76 8teenies.com 91.83.237.41 94.242.252.77 absolutesuccess.su analbreakers.com angelsnu.com anilos.com babesandstars.com babesmachine.com babi.su babushky.ru banan.in bananateens.com bravotube.net brazzers.com brbpics.com cocku.net deffki.su deviantclip.com dojki.com dreamfilth.com empflix.com erotikax.ru erovid.org exgirlsss.org exposedwebcams.com fotofaza.com fovoritki.com free-abbywinters.com fuckday.ru galleries.payserve.com ");
            sb.AppendLine(@"Porno = galleryarea.com gallsforpleasure.com girlstop.info glamursgirls.ru hardsextube.com hornygf.net inferalton.com innocentcute.com juicyads.com karups kashtanka.com lustyguide.com massage-bagira.com.ua mature-beauty.com maturegoldenladies.com maw.ru MAXIM minuet.biz modelsnu.com mybabes.com myshyteens.com nagishom.org naked nastyteens.net nubiles.net nude nudist-colony.org NUTS nylonx.net onlyamateursteens.com osiskax.com prorvasex.com pussycash.com xxx tits porno xuy.com paikry7.narod.ru penthouse pinkmature.com playboy podlectube.com pokazuha.ru popka. porn powersex.ru ");
            sb.AppendLine(@"Porno = realitykings.com redtube.com runetki. seks sensualgirls.org sexa. sexy shufuni.com solokittens.com soscka.ru spermian.com spy2wc.org teen-angels.org teenartclub.net teenport.com theteens.org tinysolo.com tnaflix.com trahun.tv tube8.com tusnya.net tygiepopki.com ubka.zadniza.com upskirt video-girl.tv vidz.com v-razvrate.org wetplace.com mybestfetish.com xvideo XXL XXX xyu.tv xyya.net yellowmedia.biz yobt.tv youjizz.com young-n-fetish.com yourlust.com suero.tv erotixkachky.nestkwell.ru ero-x.com brazzers.com chastnoe.net flv.pteranoz.ru lopso.net ruwrz.ru foto-golykh.ru sex.borzna.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Games = .war 11x11.ru 188.93.63.90 85.214.84.91 bigler desert-operations. flashhome.ru game inviziads klanz.ru mir-stalkera.ru myazart.com playground.ru romadoria.ru thesaints.info travian uo.net.ua worldoftanks.net wargaming.net xcraft. zgncdn.com zynga.com vk.angrypets.ru tankionline.com ag.ru onlineguru.ru mochiads.com kiwzi.net igrofania.ru warthunder.ru playjournal.ru skillclub.com playstation. bungie.net gaming igromania.ru playtomic.com ru-wotp.wgcdn.co ");
            sb.AppendLine(@"Guns = abrams.com.ua airgun.org.ua allzip.org guns.ru gunshop.com.ua ibis.net.ua maksnipe.kiev.ua militarist. opoccuu.com pmcjournal.com russianguns.ru 3mv.ru topwar.ru gearshout.net guns02.ru wiking.kiev.ua ukrspecexport.com reibert.info voentorg.ua guns.ua ohotniki.ru ohotnik.com stvol.ua knifeclub.com.ua knife.com.ua guns ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Anonimize = translate.google. translate.yandex.ru anonymizer anonimizing hide anonymouse obhodilka.ru omg5.ru prolezayka.ru blaim.ru vkvezde.ru vkontaktir.ru vkhodi.ru dd34.ru cmle.ru 1proxy.de erenta.ru bremdy.ru biglu.ru oknovpope.ru nblu.ru noblockme anonim.pro pingway.ru kalarupa.com 2ip.ru cameleo.ru proxfree proxyweb 3proxy.de daidostup.ru leader.ru hidemy ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Virus = .rackcdn.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"VideoSurveillance = golden-eye.com.ua");
            sb.AppendLine(@";End Direction");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";Disciption of the Direction.  ");
            sb.AppendLine(@"# Описание Направлений URL");
            sb.AppendLine(@"Anonimize = Анонимайзеры (скрытие посещаемого URL)");
            sb.AppendLine(@"AutoClubs = Автоклубы и автофорумы");
            sb.AppendLine(@"AutoLogistika = Автологистические компании");
            sb.AppendLine(@"Banks = Банки и платежные системы");
            sb.AppendLine(@"Totalizatos = Тотализаторы - игры с ценными бумагами");
            sb.AppendLine(@"Cars = Автопроизводители и автодилеры");
            sb.AppendLine(@"DigitalAds = Интернет реклама");
            sb.AppendLine(@"Ebooksmagazine = Электронные журналы и книги");
            sb.AppendLine(@"FileStores = Файловые хранилища");
            sb.AppendLine(@"Finders = Поисковые сервера");
            sb.AppendLine(@"Funny = Развлечения");
            sb.AppendLine(@"Games = Игровые сервера");
            sb.AppendLine(@"GPSCar = GPS - навигация - карты");
            sb.AppendLine(@"Guns = Оружие и военная тематика");
            sb.AppendLine(@"HelthSportBeauty = Здоровье - красота - спорт");
            sb.AppendLine(@"Ittech = Информационные технологии");
            sb.AppendLine(@"LookforaJob = Поиск работы");
            sb.AppendLine(@"Mailers = E-Mail сервисы");
            sb.AppendLine(@"MusicRadioonline = Музыка и радио онлайн");
            sb.AppendLine(@"NewsInfoAds = Новости - информация - объявления");
            sb.AppendLine(@"Parts = Автозапчасти и СТО");
            sb.AppendLine(@"Porno = Клубничка");
            sb.AppendLine(@"Profiles = Профильные направления (бухгалтерия - кадры - юридические - таможенные)");
            sb.AppendLine(@"Shops = Магазины");
            sb.AppendLine(@"ShopBoutiq = Бутики");
            sb.AppendLine(@"ShopBuild = Строительные сайты ");
            sb.AppendLine(@"ShopDigital = Магазины цифровой техники");
            sb.AppendLine(@"ShopRieltor = Покупка - продажа - аренда недвижимости");
            sb.AppendLine(@"SocialNets = Социальные сети");
            sb.AppendLine(@"Teach = Обучение");
            sb.AppendLine(@"VideoTV = Видео и телевидение онлайн");
            sb.AppendLine(@"Images = Файловые хранилища изображений");
            sb.AppendLine(@"Common = Категория неопределенна");
            sb.AppendLine(@"Virus = Вирусный сайт - ПК ЗАРАЖЕН");
            sb.AppendLine(@"VideoSurveillance = Видеонаблюдение (железо, ПО и услуги)");
            sb.AppendLine(@";End Disciption of the Direction");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";Cleaner.");
            sb.AppendLine(@"# Удаление мусора из ссылок. Только один набор на строку!");
            sb.AppendLine(@"www.");
            sb.AppendLine(@"# :21");
            sb.AppendLine(@"*.");
            sb.AppendLine(@";End Cleaner");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";Replacer.");
            sb.AppendLine(@"# Замена URL содержащей  домен указанный с правой стороны знака  =  на другой указанный перед знаком = ");
            sb.AppendLine(@"# Перебор доменов идет слева направо");
            sb.AppendLine(@"# После нахождения первого совпадения перебор прекращается");
            sb.AppendLine(@"");
            sb.AppendLine(@"kinogo.club = kinogo.club kinogo.co kinogo.cc kinogo.by ");
            sb.AppendLine(@"moonwalk.cc = moonwalk.cc moonwalk.co ");
            sb.AppendLine(@"facebook.net = facebook.net facebook.com ");
            sb.AppendLine(@"soundcloud.com = soundcloud.com cf-hls-media.sndcdn.com ");
            sb.AppendLine(@"google.com = google.com.ua google.com.ru safebrowsing-cache.google.com google.com ");
            sb.AppendLine(@"yandex.ru = yandex.ru yandex.net yandex.ua ");
            sb.AppendLine(@"wargaming.net = wargaming.net wargaming.ua wargaming.ru ");
            sb.AppendLine(@"worldoftanks.net = worldoftanks.net worldoftanks.ua worldoftanks.ru");
            sb.AppendLine(@"4pda.ru = 4pda.ru 4pda.to ");
            sb.AppendLine(@"kinokrad.net = kinokrad.net kinokrad.co ");
            sb.AppendLine(@"criteo.net = criteo.com");
            sb.AppendLine(@";End Replacer");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";Simplifier.");
            sb.AppendLine(@"# Если URL содержит домен указанный ниже URL заменяется на указанный");
            sb.AppendLine(@"101.ru");
            sb.AppendLine(@"1plus1.video");
            sb.AppendLine(@"24video.adult");
            sb.AppendLine(@"adio.obozrevatel.com");
            sb.AppendLine(@"adme.ru");
            sb.AppendLine(@"akamaihd.net");
            sb.AppendLine(@"alicdn.com");
            sb.AppendLine(@"aliexpress.com");
            sb.AppendLine(@"amazonaws.com");
            sb.AppendLine(@"anonimizing.com");
            sb.AppendLine(@"anwap.org");
            sb.AppendLine(@"apollostream.xyz");
            sb.AppendLine(@"auto.drom.ru");
            sb.AppendLine(@"auto.ria.com");
            sb.AppendLine(@"autogidas.lt");
            sb.AppendLine(@"bonus-tv.ru");
            sb.AppendLine(@"carsontheweb.com");
            sb.AppendLine(@"cdn.riastatic.com");
            sb.AppendLine(@"cdn.yandex.ru");
            sb.AppendLine(@"cdnvideo.ru");
            sb.AppendLine(@"censor.net.ua");
            sb.AppendLine(@"cf5.rackcdn.com");
            sb.AppendLine(@"chocoapp.ru");
            sb.AppendLine(@"citrus.ua");
            sb.AppendLine(@"cosmopolitan.ru");
            sb.AppendLine(@"criteo.net");
            sb.AppendLine(@"d-cd.net");
            sb.AppendLine(@"deezee.pl");
            sb.AppendLine(@"doubleclick.net");
            sb.AppendLine(@"dropmefiles.com");
            sb.AppendLine(@"edisk.ukr.net");
            sb.AppendLine(@"estafeta.org");
            sb.AppendLine(@"facebook.net");
            sb.AppendLine(@"files.attachmail.ru");
            sb.AppendLine(@"fishki.net");
            sb.AppendLine(@"forexpros.com");
            sb.AppendLine(@"fotocdn.net");
            sb.AppendLine(@"githubusercontent.com");
            sb.AppendLine(@"golden-eye.com.ua");
            sb.AppendLine(@"gvt1.com");
            sb.AppendLine(@"hotcloud.org");
            sb.AppendLine(@"imgsmail.ru");
            sb.AppendLine(@"instagram.com");
            sb.AppendLine(@"intercom.io");
            sb.AppendLine(@"kaban.tv");
            sb.AppendLine(@"kamaized.net");
            sb.AppendLine(@"kinogo.club");
            sb.AppendLine(@"kinokrad.net");
            sb.AppendLine(@"lanet.tv");
            sb.AppendLine(@"leboutique.com");
            sb.AppendLine(@"ligazakon.net");
            sb.AppendLine(@"1internet.tv");
            sb.AppendLine(@"makeagif.com");
            sb.AppendLine(@"maps.yandex.ru");
            sb.AppendLine(@"marketgid.com");
            sb.AppendLine(@"macc.com.ua");
            sb.AppendLine(@"media.online.ua");
            sb.AppendLine(@"mixcloud.com");
            sb.AppendLine(@"mixupload.com");
            sb.AppendLine(@"moonwalk.cc");
            sb.AppendLine(@"my.mail.ru");
            sb.AppendLine(@"muzofond.org");
            sb.AppendLine(@"mp3ton.info");
            sb.AppendLine(@"mycdn.me");
            sb.AppendLine(@"mp3xa.pw");
            sb.AppendLine(@"myzuka.me");
            sb.AppendLine(@"mzstatic.com");
            sb.AppendLine(@"nblu.ru");
            sb.AppendLine(@"obozrevatel.ua");
            sb.AppendLine(@"ollcdn.net");
            sb.AppendLine(@"olx.ua");
            sb.AppendLine(@"onlineradiobox.com");
            sb.AppendLine(@"ovva.tv");
            sb.AppendLine(@"pdbcdn.co");
            sb.AppendLine(@"pikabu.ru");
            sb.AppendLine(@"planeta-online.tv");
            sb.AppendLine(@"playstation.com");
            sb.AppendLine(@"playtomic.com");
            sb.AppendLine(@"porno365.xxx");
            sb.AppendLine(@"presenta.xyz");
            sb.AppendLine(@"rackcdn.com");
            sb.AppendLine(@"redhat.com");
            sb.AppendLine(@"ringon.ru");
            sb.AppendLine(@"riastatic.com");
            sb.AppendLine(@"rozetka.ua");
            sb.AppendLine(@"runetki.co");
            sb.AppendLine(@"rutube.ru");
            sb.AppendLine(@"sender.mobi");
            sb.AppendLine(@"shutterstock.com");
            sb.AppendLine(@"spincar.com");
            sb.AppendLine(@"soundcloud.com");
            sb.AppendLine(@"spac.me");
            sb.AppendLine(@"spaces.ru");
            sb.AppendLine(@"storage.yandex.ru");
            sb.AppendLine(@"tavrmedia.ua");
            sb.AppendLine(@"testlivestream.rfn.ru");
            sb.AppendLine(@"thesame.tv");
            sb.AppendLine(@"trofey.net");
            sb.AppendLine(@"ttvnw.net");
            sb.AppendLine(@"turbobit.net");
            sb.AppendLine(@"tvigle.ru");
            sb.AppendLine(@"tvzavr.ru");
            sb.AppendLine(@"twitch.tv");
            sb.AppendLine(@"vcdn.biz");
            sb.AppendLine(@"videoprobki.com.ua");
            sb.AppendLine(@"vidyomani.com");
            sb.AppendLine(@"vimeocdn.com");
            sb.AppendLine(@"# VisualStudio Extension");
            sb.AppendLine(@"#vsassets.io");
            sb.AppendLine(@"#visualstudio.com");
            sb.AppendLine(@"xvideos.com");
            sb.AppendLine(@"yapfiles.ru");
            sb.AppendLine(@"yaplakal.com");
            sb.AppendLine(@"yaporn.sex");
            sb.AppendLine(@"youtube.com");
            sb.AppendLine(@"ytimg.com");
            sb.AppendLine(@"wargaming.net");
            sb.AppendLine(@"# Windows Update");
            sb.AppendLine(@"windowsupdate.com");
            sb.AppendLine(@"update.microsoft.com");
            sb.AppendLine(@"zaycev.net");
            sb.AppendLine(@"zf.fm");
            sb.AppendLine(@"myradio24.com");
            sb.AppendLine(@"stranamam.ru");
            sb.AppendLine(@"postila.ru");
            sb.AppendLine(@";End Simplifier");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";SimplifyEnd");
            sb.AppendLine(@"# Если URL заканчивается на указанный ниже, то отрезается часть URL спереди до указанной нижн маски");
            sb.AppendLine(@"vimeo.akamaized.net");
            sb.AppendLine(@"bankvostok.com.ua");
            sb.AppendLine(@";End SimplifyEnd");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@"; End of Configuration");
            File.WriteAllText("ProxyAnalyser.ini", sb.ToString(), System.Text.Encoding.GetEncoding(1251));
            sb = null;
        }
    }

}
