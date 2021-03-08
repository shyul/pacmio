﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// Interactive Brokers API
/// 
/// ***************************************************************************

using System;
using System.Reflection;
using Xu;

namespace Pacmio.IB
{
    public static partial class Client
    {
        private static bool IsReady_MktDepthExchanges { get; set; } = true;

        public static bool SendRequest_MktDepthExchanges()
        {
            if (IsReady_MktDepthExchanges)
            {
                IsReady_MktDepthExchanges = false;

                SendRequest(new string[] {
                    ((int)RequestType.RequestMktDepthExchanges).ToString(), // 62
                });

                return true;
            }
            return false;
        }

        /// <summary>
        ///         RequestMktDepthExchanges = 82,
        /// 
        /// </summary>
        /// <param name="fields"></param>
        private static void Parse_MktDepthExchanges(string[] fields)
        {
            IsReady_MktDepthExchanges = true;
            int num = fields[1].ToInt32();

            lock (Parameters)
            {
                for (int i = 0; i < num; i++)
                {
                    int pt = (i * 5) + 2;

                    string exchangeCode = fields[pt];
                    string typeCode = fields[pt + 1];
                    string listExchange = fields[pt + 2];
                    string serviceDataType = fields[pt + 3];
                    int aggGroup = fields[pt + 4].ToInt32();

                    Parameters.ExchangeDescription[(exchangeCode, typeCode)] = (listExchange, serviceDataType, aggGroup);

                    Console.WriteLine(exchangeCode + ": " + typeCode + " | " + serviceDataType);
                }
            }

            //Console.WriteLine(MethodBase.GetCurrentMethod().Name + ": " + fields.ToStringWithIndex());
        }


    }
}

/*
 Parse_MktDepthExchanges: (0)"80"-(1)"246"-(2)"DTB"-(3)"OPT"-(4)""-(5)"Deep"-(6)"2147483647"-(7)"LSEETF"-(8)"STK"-(9)""-(10)"Deep"-(11)"2147483647"-(12)"SGX"-(13)"FUT"-(14)""-(15)"Deep"-(16)"2147483647"-(17)"IDEALPRO"-(18)"CASH"-(19)""-(20)"Deep"-(21)"4"-(22)"ARCA"-(23)"STK"-(24)""-(25)"Deep"-(26)"2147483647"-(27)"SEHK"-(28)"WAR"-(29)""-(30)"DeepX"-(31)"2147483647"-(32)"AEB"-(33)"IOPT"-(34)""-(35)"Deep"-(36)"2147483647"-(37)"NASDTAS"-(38)"STK"-(39)""-(40)"Deep"-(41)"2147483647"-(42)"CHIXDK"-(43)"STK"-(44)""-(45)"Deep"-(46)"2147483647"-(47)"NSE"-(48)"FUT"-(49)""-(50)"Deep"-(51)"2147483647"-(52)"OSE.JPN"-(53)"FUT"-(54)""-(55)"Deep"-(56)"2147483647"-(57)"ICEEUSOFT"-(58)"FUT"-(59)""-(60)"Deep"-(61)"2147483647"-(62)"BATEEN"-(63)"STK"-(64)""-(65)"Deep"-(66)"2147483647"-(67)"DXENO"-(68)"STK"-(69)""-(70)"Deep"-(71)"2147483647"-(72)"AMEXB"-(73)"OPT"-(74)""-(75)"Deep"-(76)"2147483647"-(77)"IDEM"-(78)"OPT"-(79)""-(80)"Deep"-(81)"2147483647"-(82)"IBIS2"-(83)"STK"-(84)""-(85)"Deep"-(86)"2147483647"-(87)"TRQXDE"-(88)"STK"-(89)""-(90)"Deep"-(91)"2147483647"-(92)"ENEXT.BE"-(93)"STK"-(94)""-(95)"Deep"-(96)"2147483647"-(97)"BVME"-(98)"STK"-(99)""-(100)"Deep"-(101)"2147483647"-(102)"OMH"-(103)"FUT"-(104)""-(105)"Deep"-(106)"2147483647"-(107)"BVME.ETF"-(108)"STK"-(109)""-(110)"Deep"-(111)"2147483647"-(112)"BELFOX"-(113)"FUT"-(114)""-(115)"Deep"-(116)"2147483647"-(117)"CHIXFI"-(118)"STK"-(119)""-(120)"Deep"-(121)"2147483647"-(122)"NYSE"-(123)"WAR"-(124)""-(125)"Deep"-(126)"2147483647"-(127)"TGHEHU"-(128)"STK"-(129)""-(130)"Deep"-(131)"2147483647"-(132)"CHIXSE"-(133)"STK"-(134)""-(135)"Deep"-(136)"2147483647"-(137)"BMF"-(138)"FUT"-(139)""-(140)"Deep"-(141)"2147483647"-(142)"CDE"-(143)"FUT"-(144)""-(145)"Deep"-(146)"2147483647"-(147)"CHIXES"-(148)"STK"-(149)""-(150)"Deep"-(151)"2147483647"-(152)"TRQXUK"-(153)"STK"-(154)""-(155)"Deep"-(156)"2147483647"-(157)"KSE"-(158)"STK"-(159)""-(160)"Deep"-(161)"2147483647"-(162)"TGHEFI"-(163)"STK"-(164)""-(165)"Deep"-(166)"2147483647"-(167)"EUREXCBL"-(168)"OPT"-(169)""-(170)"Deep"-(171)"2147483647"-(172)"KSE"-(173)"OPT"-(174)""-(175)"Deep"-(176)"2147483647"-(177)"OMXNO"-(178)"FUT"-(179)""-(180)"Deep"-(181)"2147483647"-(182)"CHIXNO"-(183)"STK"-(184)""-(185)"Deep"-(186)"2147483647"-(187)"BATESE"-(188)"STK"-(189)""-(190)"Deep"-(191)"2147483647"-(192)"TRQXPL"-(193)"STK"-(194)""-(195)"Deep"-(196)"2147483647"-(197)"MATIF"-(198)"FUT"-(199)""-(200)"Deep"-(201)"2147483647"-(202)"TOM"-(203)"OPT"-(204)""-(205)"Deep"-(206)"2147483647"-(207)"DXEDK"-(208)"STK"-(209)""-(210)"Deep"-(211)"2147483647"-(212)"MATIF"-(213)"FOP"-(214)""-(215)"Deep"-(216)"2147483647"-(217)"GLOBEX"-(218)"BAG"-(219)""-(220)"Deep"-(221)"2147483647"-(222)"ICECRYPTO"-(223)"FUT"-(224)""-(225)"Deep"-(226)"2147483647"-(227)"TGHEES"-(228)"STK"-(229)""-(230)"Deep"-(231)"2147483647"-(232)"DXEEN"-(233)"STK"-(234)""-(235)"Deep"-(236)"2147483647"-(237)"CHIXHU"-(238)"STK"-(239)""-(240)"Deep"-(241)"2147483647"-(242)"SOFFEX"-(243)"FUT"-(244)""-(245)"Deep"-(246)"2147483647"-(247)"TGHEDK"-(248)"STK"-(249)""-(250)"Deep"-(251)"2147483647"-(252)"TSE"-(253)"STK"-(254)""-(255)"Deep"-(256)"2147483647"-(257)"TGHENO"-(258)"STK"-(259)""-(260)"Deep"-(261)"2147483647"-(262)"TSEJ"-(263)"FOP"-(264)""-(265)"Deep"-(266)"2147483647"-(267)"MERCURY"-(268)"OPT"-(269)""-(270)"Deep"-(271)"2147483647"-(272)"ERIS"-(273)"FUT"-(274)""-(275)"Deep"-(276)"2147483647"-(277)"ISE"-(278)"OPT"-(279)""-(280)"Deep"-(281)"2147483647"-(282)"LMEOTC"-(283)"FUT"-(284)""-(285)"Deep"-(286)"2147483647"-(287)"TRQXEN"-(288)"STK"-(289)""-(290)"Deep"-(291)"2147483647"-(292)"SEHK"-(293)"STK"-(294)""-(295)"Deep"-(296)"2147483647"-(297)"IDEM"-(298)"FUT"-(299)""-(300)"Deep"-(301)"2147483647"-(302)"TSE"-(303)"WAR"-(304)""-(305)"Deep"-(306)"2147483647"-(307)"CHIXEN"-(308)"STK"-(309)""-(310)"Deep"-(311)"2147483647"-(312)"BELFOX"-(313)"OPT"-(314)""-(315)"Deep"-(316)"2147483647"-(317)"IBEFP"-(318)"BAG"-(319)""-(320)"Deep2"-(321)"2147483647"-(322)"DXEHU"-(323)"STK"-(324)""-(325)"Deep"-(326)"2147483647"-(327)"WSE"-(328)"FUT"-(329)""-(330)"Deep"-(331)"2147483647"-(332)"TSEJ"-(333)"OPT"-(334)""-(335)"Deep"-(336)"2147483647"-(337)"BATEDK"-(338)"STK"-(339)""-(340)"Deep"-(341)"2147483647"-(342)"CFE"-(343)"FUT"-(344)""-(345)"Deep"-(346)"2147483647"-(347)"ECBOT"-(348)"FOP"-(349)""-(350)"Deep"-(351)"2147483647"-(352)"NSE"-(353)"OPT"-(354)""-(355)"Deep"-(356)"2147483647"-(357)"TRQXCH"-(358)"STK"-(359)""-(360)"Deep"-(361)"2147483647"-(362)"BATENO"-(363)"STK"-(364)""-(365)"Deep"-(366)"2147483647"-(367)"NYMEX"-(368)"FUT"-(369)""-(370)"Deep"-(371)"2147483647"-(372)"MONEP"-(373)"OPT"-(374)""-(375)"Deep"-(376)"2147483647"-(377)"TOMSBONDM"-(378)"BOND"-(379)""-(380)"Deep"-(381)"2147483647"-(382)"GEMINI"-(383)"OPT"-(384)""-(385)"Deep"-(386)"2147483647"-(387)"TRQXUK"-(388)"FUT"-(389)""-(390)"Deep"-(391)"2147483647"-(392)"AMEX"-(393)"OPT"-(394)""-(395)"Deep"-(396)"2147483647"-(397)"N.RIGA"-(398)"STK"-(399)""-(400)"Deep"-(401)"2147483647"-(402)"NSE"-(403)"STK"-(404)""-(405)"Deep"-(406)"2147483647"-(407)"PRA"-(408)"STK"-(409)""-(410)"Deep"-(411)"2147483647"-(412)"DTB"-(413)"FUT"-(414)""-(415)"Deep"-(416)"2147483647"-(417)"GLOBEX"-(418)"FUT"-(419)""-(420)"Deep"-(421)"2147483647"-(422)"CHIXCH"-(423)"STK"-(424)""-(425)"Deep"-(426)"2147483647"-(427)"TGHEDE"-(428)"STK"-(429)""-(430)"Deep"-(431)"2147483647"-(432)"EUREXCBL"-(433)"FUT"-(434)""-(435)"Deep"-(436)"2147483647"-(437)"AQS"-(438)"SLB"-(439)""-(440)"Deep"-(441)"2147483647"-(442)"TSEJ"-(443)"STK"-(444)""-(445)"Deep"-(446)"2147483647"-(447)"VIRTX"-(448)"STK"-(449)""-(450)"Deep"-(451)"2147483647"-(452)"BATEPL"-(453)"STK"-(454)""-(455)"Deep"-(456)"2147483647"-(457)"HKFE"-(458)"OPT"-(459)""-(460)"Deep"-(461)"2147483647"-(462)"ICEEU"-(463)"FUT"-(464)""-(465)"Deep"-(466)"2147483647"-(467)"BVL"-(468)"STK"-(469)""-(470)"Deep"-(471)"2147483647"-(472)"BMF"-(473)"FOP"-(474)""-(475)"Deep"-(476)"2147483647"-(477)"CHIXUK"-(478)"STK"-(479)""-(480)"Deep"-(481)"2147483647"-(482)"ISLAND"-(483)"STK"-(484)""-(485)"Deep"-(486)"2147483647"-(487)"DXEDE"-(488)"STK"-(489)""-(490)"Deep"-(491)"2147483647"-(492)"BATEDE"-(493)"STK"-(494)""-(495)"Deep"-(496)"2147483647"-(497)"SBF"-(498)"IOPT"-(499)""-(500)"Deep"-(501)"2147483647"-(502)"SGXCME"-(503)"FUT"-(504)""-(505)"Deep"-(506)"2147483647"-(507)"BATEES"-(508)"STK"-(509)""-(510)"Deep"-(511)"2147483647"-(512)"SMART"-(513)"CMDTY"-(514)""-(515)"Deep"-(516)"14"-(517)"SMART"-(518)"STK"-(519)"PINK"-(520)"AggDeep"-(521)"1"-(522)"ICEEU"-(523)"FOP"-(524)""-(525)"Deep"-(526)"2147483647"-(527)"DXEFI"-(528)"STK"-(529)""-(530)"Deep"-(531)"2147483647"-(532)"PSE"-(533)"OPT"-(534)""-(535)"Deep"-(536)"2147483647"-(537)"FTA"-(538)"OPT"-(539)""-(540)"Deep"-(541)"2147483647"-(542)"FTA"-(543)"FUT"-(544)""-(545)"Deep"-(546)"2147483647"-(547)"TOM"-(548)"FUT"-(549)""-(550)"Deep"-(551)"2147483647"-(552)"CFECRYPTO"-(553)"BAG"-(554)""-(555)"Deep"-(556)"2147483647"-(557)"ARCA"-(558)"WAR"-(559)""-(560)"Deep"-(561)"2147483647"-(562)"NYBOT"-(563)"FOP"-(564)""-(565)"Deep"-(566)"2147483647"-(567)"BEX"-(568)"WAR"-(569)""-(570)"Deep"-(571)"2147483647"-(572)"OSE.JPN"-(573)"BAG"-(574)""-(575)"Deep"-(576)"2147483647"-(577)"NYSELIFFE"-(578)"FUT"-(579)""-(580)"Deep"-(581)"2147483647"-(582)"WSE"-(583)"STK"-(584)""-(585)"Deep"-(586)"2147483647"-(587)"VENTURE"-(588)"WAR"-(589)""-(590)"Deep"-(591)"2147483647"-(592)"SMART"-(593)"BILL"-(594)""-(595)"Deep"-(596)"12"-(597)"SEHK"-(598)"IOPT"-(599)""-(600)"Deep"-(601)"2147483647"-(602)"CHIXIT"-(603)"STK"-(604)""-(605)"Deep"-(606)"2147483647"-(607)"ENEXT.BE"-(608)"IOPT"-(609)""-(610)"Deep"-(611)"2147483647"-(612)"AEB"-(613)"STK"-(614)""-(615)"Deep"-(616)"2147483647"-(617)"CMECRYPTO"-(618)"FUT"-(619)""-(620)"Deep"-(621)"2147483647"-(622)"ICECRYPTO"-(623)"BAG"-(624)""-(625)"Deep"-(626)"2147483647"-(627)"HKFE"-(628)"FUT"-(629)""-(630)"Deep"-(631)"2147483647"-(632)"ICEEU"-(633)"OPT"-(634)""-(635)"Deep"-(636)"2147483647"-(637)"SEHK"-(638)"IOPT"-(639)""-(640)"DeepX"-(641)"2147483647"-(642)"OMC"-(643)"FUT"-(644)""-(645)"Deep"-(646)"2147483647"-(647)"TRQXFI"-(648)"STK"-(649)""-(650)"Deep"-(651)"2147483647"-(652)"MERCURY"-(653)"BAG"-(654)""-(655)"Deep"-(656)"2147483647"-(657)"N.TALLINN"-(658)"STK"-(659)""-(660)"Deep"-(661)"2147483647"-(662)"MEFFRV"-(663)"FUT"-(664)""-(665)"Deep"-(666)"2147483647"-(667)"NYBOT"-(668)"FUT"-(669)""-(670)"Deep"-(671)"2147483647"-(672)"TGHEPL"-(673)"STK"-(674)""-(675)"Deep"-(676)"2147483647"-(677)"IEX"-(678)"WAR"-(679)""-(680)"Deep"-(681)"2147483647"-(682)"CFETAS"-(683)"FUT"-(684)""-(685)"Deep"-(686)"2147483647"-(687)"TSEJ"-(688)"FUT"-(689)""-(690)"Deep"-(691)"2147483647"-(692)"TRQXDK"-(693)"STK"-(694)""-(695)"Deep"-(696)"2147483647"-(697)"SEHKNTL"-(698)"STK"-(699)""-(700)"Deep"-(701)"2147483647"-(702)"DXEPL"-(703)"STK"-(704)""-(705)"Deep"-(706)"2147483647"-(707)"BATECH"-(708)"STK"-(709)""-(710)"Deep"-(711)"2147483647"-(712)"GLOBEX"-(713)"FOP"-(714)""-(715)"Deep"-(716)"2147483647"-(717)"SEHK"-(718)"BOND"-(719)""-(720)"Deep"-(721)"2147483647"-(722)"MONEP"-(723)"FUT"-(724)""-(725)"Deep"-(726)"2147483647"-(727)"BATS"-(728)"WAR"-(729)""-(730)"Deep"-(731)"2147483647"-(732)"VENTURE"-(733)"STK"-(734)""-(735)"Deep"-(736)"2147483647"-(737)"N.VILNIUS"-(738)"STK"-(739)""-(740)"Deep"-(741)"2147483647"-(742)"DXEES"-(743)"STK"-(744)""-(745)"Deep"-(746)"2147483647"-(747)"CFE"-(748)"BAG"-(749)""-(750)"Deep"-(751)"2147483647"-(752)"SEHKSZSE"-(753)"STK"-(754)""-(755)"Deep"-(756)"2147483647"-(757)"TRQXHU"-(758)"STK"-(759)""-(760)"Deep"-(761)"2147483647"-(762)"ISLAND"-(763)"WAR"-(764)""-(765)"Deep2"-(766)"2147483647"-(767)"KSE"-(768)"FUT"-(769)""-(770)"Deep"-(771)"2147483647"-(772)"ZERO"-(773)"STK"-(774)""-(775)"Deep"-(776)"2147483647"-(777)"NYMEX"-(778)"FOP"-(779)""-(780)"Deep"-(781)"2147483647"-(782)"ONE"-(783)"BAG"-(784)""-(785)"Deep"-(786)"2147483647"-(787)"PUREMATCH"-(788)"STK"-(789)""-(790)"Deep"-(791)"2147483647"-(792)"OCXBETS"-(793)"BAG"-(794)""-(795)"Deep"-(796)"2147483647"-(797)"ASX"-(798)"FUT"-(799)""-(800)"Deep"-(801)"2147483647"-(802)"SOFFEX"-(803)"OPT"-(804)""-(805)"Deep"-(806)"2147483647"-(807)"OMXNO"-(808)"STK"-(809)""-(810)"Deep"-(811)"2147483647"-(812)"TRQXIT"-(813)"STK"-(814)""-(815)"Deep"-(816)"2147483647"-(817)"BATS"-(818)"STK"-(819)""-(820)"Deep"-(821)"2147483647"-(822)"BUX"-(823)"STK"-(824)""-(825)"Deep"-(826)"2147483647"-(827)"CFECRYPTO"-(828)"FUT"-(829)""-(830)"Deep"-(831)"2147483647"-(832)"LSEIOB1"-(833)"STK"-(834)""-(835)"Deep"-(836)"2147483647"-(837)"ASX"-(838)"BOND"-(839)""-(840)"Deep"-(841)"2147483647"-(842)"ISLAND"-(843)"STK"-(844)""-(845)"Deep2"-(846)"2147483647"-(847)"SMART"-(848)"STK"-(849)"OTCBB"-(850)"AggDeep"-(851)"1"-(852)"OMXNO"-(853)"OPT"-(854)""-(855)"Deep"-(856)"2147483647"-(857)"TOMSBOND"-(858)"BOND"-(859)""-(860)"Deep"-(861)"2147483647"-(862)"LTSE"-(863)"WAR"-(864)""-(865)"Deep"-(866)"2147483647"-(867)"ECBOT"-(868)"FUT"-(869)""-(870)"Deep"-(871)"2147483647"-(872)"PINK"-(873)"WAR"-(874)""-(875)"Deep"-(876)"2147483647"-(877)"NASDAQOM"-(878)"OPT"-(879)""-(880)"Deep"-(881)"2147483647"-(882)"SNFE"-(883)"FOP"-(884)""-(885)"Deep"-(886)"2147483647"-(887)"HEX"-(888)"STK"-(889)""-(890)"Deep"-(891)"2147483647"-(892)"BATEUK"-(893)"STK"-(894)""-(895)"Deep"-(896)"2147483647"-(897)"LSE"-(898)"STK"-(899)""-(900)"Deep"-(901)"2147483647"-(902)"OTOB"-(903)"FUT"-(904)""-(905)"Deep"-(906)"2147483647"-(907)"TRQXSE"-(908)"STK"-(909)""-(910)"Deep"-(911)"2147483647"-(912)"ASX"-(913)"OPT"-(914)""-(915)"Deep"-(916)"2147483647"-(917)"SBF"-(918)"STK"-(919)""-(920)"Deep"-(921)"2147483647"-(922)"SEHK"-(923)"WAR"-(924)""-(925)"Deep"-(926)"2147483647"-(927)"SEHK"-(928)"BOND"-(929)""-(930)"DeepX"-(931)"2147483647"-(932)"IEX"-(933)"STK"-(934)""-(935)"Deep"-(936)"2147483647"-(937)"TGHEEN"-(938)"STK"-(939)""-(940)"Deep"-(941)"2147483647"-(942)"BEX"-(943)"STK"-(944)""-(945)"Deep"-(946)"2147483647"-(947)"CMECRYPTO"-(948)"BAG"-(949)""-(950)"Deep"-(951)"2147483647"-(952)"ARCAEDGE"-(953)"WAR"-(954)""-(955)"Deep"-(956)"2147483647"-(957)"BATEIT"-(958)"STK"-(959)""-(960)"Deep"-(961)"2147483647"-(962)"VSE"-(963)"STK"-(964)""-(965)"Deep"-(966)"2147483647"-(967)"BATEHU"-(968)"STK"-(969)""-(970)"Deep"-(971)"2147483647"-(972)"ONE"-(973)"FUT"-(974)""-(975)"Deep"-(976)"2147483647"-(977)"AMEX"-(978)"BAG"-(979)""-(980)"Deep"-(981)"2147483647"-(982)"IPE"-(983)"FUT"-(984)""-(985)"Deep"-(986)"2147483647"-(987)"OMS"-(988)"FUT"-(989)""-(990)"Deep"-(991)"2147483647"-(992)"EUREXUK"-(993)"OPT"-(994)""-(995)"Deep"-(996)"2147483647"-(997)"OSE.JPN"-(998)"OPT"-(999)""-(1000)"Deep"-(1001)"2147483647"-(1002)"ASX"-(1003)"STK"-(1004)""-(1005)"Deep"-(1006)"2147483647"-(1007)"SNFE"-(1008)"FUT"-(1009)""-(1010)"Deep"-(1011)"2147483647"-(1012)"EURONEXT"-(1013)"BOND"-(1014)""-(1015)"Deep"-(1016)"2147483647"-(1017)"OSE.JPN"-(1018)"FOP"-(1019)""-(1020)"Deep"-(1021)"2147483647"-(1022)"TAIFEX"-(1023)"FUT"-(1024)""-(1025)"Deep"-(1026)"2147483647"-(1027)"MEFFRV"-(1028)"OPT"-(1029)""-(1030)"Deep"-(1031)"2147483647"-(1032)"OCXBETS"-(1033)"FUT"-(1034)""-(1035)"Deep"-(1036)"2147483647"-(1037)"PREBORROW"-(1038)"SLB"-(1039)""-(1040)"Deep"-(1041)"2147483647"-(1042)"CMECRYPTO"-(1043)"IND"-(1044)""-(1045)"Deep"-(1046)"2147483647"-(1047)"BOVESPA"-(1048)"OPT"-(1049)""-(1050)"Deep"-(1051)"2147483647"-(1052)"SEHK"-(1053)"STK"-(1054)""-(1055)"DeepX"-(1056)"2147483647"-(1057)"CHINEXT"-(1058)"STK"-(1059)""-(1060)"Deep"-(1061)"2147483647"-(1062)"AQS_P"-(1063)"SLB"-(1064)""-(1065)"Deep"-(1066)"2147483647"-(1067)"ISLAND"-(1068)"WAR"-(1069)""-(1070)"Deep"-(1071)"2147483647"-(1072)"TSEJ"-(1073)"BAG"-(1074)""-(1075)"Deep"-(1076)"2147483647"-(1077)"CHIXAT"-(1078)"STK"-(1079)""-(1080)"Deep"-(1081)"2147483647"-(1082)"ARCAEDGE"-(1083)"STK"-(1084)""-(1085)"Deep"-(1086)"2147483647"-(1087)"BOX"-(1088)"OPT"-(1089)""-(1090)"Deep"-(1091)"2147483647"-(1092)"CFECRYPTO"-(1093)"IND"-(1094)""-(1095)"Deep"-(1096)"2147483647"-(1097)"TRQXAT"-(1098)"STK"-(1099)""-(1100)"Deep"-(1101)"2147483647"-(1102)"ICEUS"-(1103)"FUT"-(1104)""-(1105)"Deep"-(1106)"2147483647"-(1107)"BM"-(1108)"STK"-(1109)""-(1110)"Deep"-(1111)"2147483647"-(1112)"LSEIOB2"-(1113)"STK"-(1114)""-(1115)"Deep"-(1116)"2147483647"-(1117)"SMART"-(1118)"BOND"-(1119)""-(1120)"Deep"-(1121)"7"-(1122)"OMXNO"-(1123)"IND"-(1124)""-(1125)"Deep"-(1126)"2147483647"-(1127)"CHIXPL"-(1128)"STK"-(1129)""-(1130)"Deep"-(1131)"2147483647"-(1132)"TRQXNO"-(1133)"STK"-(1134)""-(1135)"Deep"-(1136)"2147483647"-(1137)"SFB"-(1138)"STK"-(1139)""-(1140)"Deep"-(1141)"2147483647"-(1142)"OSE"-(1143)"STK"-(1144)""-(1145)"Deep"-(1146)"2147483647"-(1147)"LTSE"-(1148)"STK"-(1149)""-(1150)"Deep"-(1151)"2147483647"-(1152)"DXESE"-(1153)"STK"-(1154)""-(1155)"Deep"-(1156)"2147483647"-(1157)"CHIXDE"-(1158)"STK"-(1159)""-(1160)"Deep"-(1161)"2147483647"-(1162)"IBIS"-(1163)"STK"-(1164)""-(1165)"Deep"-(1166)"2147483647"-(1167)"NYSE"-(1168)"STK"-(1169)""-(1170)"Deep"-(1171)"2147483647"-(1172)"EDXNO"-(1173)"FUT"-(1174)""-(1175)"Deep"-(1176)"2147483647"-(1177)"PINK"-(1178)"STK"-(1179)""-(1180)"Deep"-(1181)"2147483647"-(1182)"BATEFI"-(1183)"STK"-(1184)""-(1185)"Deep"-(1186)"2147483647"-(1187)"PSE"-(1188)"BAG"-(1189)""-(1190)"Deep"-(1191)"2147483647"-(1192)"TGHESE"-(1193)"STK"-(1194)""-(1195)"Deep"-(1196)"2147483647"-(1197)"EBS"-(1198)"STK"-(1199)""-(1200)"Deep"-(1201)"2147483647"-(1202)"GEMINI"-(1203)"BAG"-(1204)""-(1205)"Deep"-(1206)"2147483647"-(1207)"TRQXES"-(1208)"STK"-(1209)""-(1210)"Deep"-(1211)"2147483647"-(1212)"CPH"-(1213)"STK"-(1214)""-(1215)"Deep"-(1216)"2147483647"-(1217)"CDE"-(1218)"OPT"-(1219)""-(1220)"Deep"-(1221)"2147483647"-(1222)"ISE"-(1223)"BAG"-(1224)""-(1225)"Deep"-(1226)"2147483647"-(1227)"TRQXIR"-(1228)"STK"-(1229)""-(1230)"Deep"-(1231)"2147483647"
 
 */