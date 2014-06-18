using System;
using System.Globalization;
using NUnit.Framework;

namespace Catalogue.Tests
{/*
    aar	aar	aa	Afar	Individual	Living
aav			Austro-Asiatic languages	Collective	
abk	abk	ab	Abkhazian	Individual	Living
ace	ace		Achinese	Individual	Living
ach	ach		Acoli	Individual	Living
ada	ada		Adangme	Individual	Living
ady	ady		Adyghe	Individual	Living
afa			Afro-Asiatic languages	Collective	
afh	afh		Afrihili	Individual	Constructed
afr	afr	af	Afrikaans	Individual	Living
ain	ain		Ainu (Japan)	Individual	Living
aka	aka	ak	Akan	Macrolanguage	Living
akk	akk		Akkadian	Individual	Ancient
alb* / sqi	sqi	sq	Albanian	Macrolanguage	Living
ale	ale		Aleut	Individual	Living
alg			Algonquian languages	Collective	
alt	alt		Southern Altai	Individual	Living
alv			Atlantic-Congo languages	Collective	
amh	amh	am	Amharic	Individual	Living
ang	ang		Old English (ca. 450-1100)	Individual	Historical
anp	anp		Angika	Individual	Living
apa			Apache languages	Collective	
aqa			Alacalufan languages	Collective	
aql			Algic languages	Collective	
ara	ara	ar	Arabic	Macrolanguage	Living
arc	arc		Official Aramaic (700-300 BCE)	Individual	Ancient
arg	arg	an	Aragonese	Individual	Living
arm* / hye	hye	hy	Armenian	Individual	Living
arn	arn		Mapudungun	Individual	Living
arp	arp		Arapaho	Individual	Living
art			Artificial languages	Collective	
arw	arw		Arawak	Individual	Living
asm	asm	as	Assamese	Individual	Living
ast	ast		Asturian	Individual	Living
ath			Athapascan languages	Collective	
auf			Arauan languages	Collective	
aus			Australian languages	Collective	
ava	ava	av	Avaric	Individual	Living
ave	ave	ae	Avestan	Individual	Ancient
awa	awa		Awadhi	Individual	Living
awd			Arawakan languages	Collective	
aym	aym	ay	Aymara	Macrolanguage	Living
azc			Uto-Aztecan languages	Collective	
aze	aze	az	Azerbaijani	Macrolanguage	Living
bad			Banda languages	Collective	
bai			Bamileke languages	Collective	
bak	bak	ba	Bashkir	Individual	Living
bal	bal		Baluchi	Macrolanguage	Living
bam	bam	bm	Bambara	Individual	Living
ban	ban		Balinese	Individual	Living
baq* / eus	eus	eu	Basque	Individual	Living
bas	bas		Basa (Cameroon)	Individual	Living
bat			Baltic languages	Collective	
bej	bej		Beja	Individual	Living
bel	bel	be	Belarusian	Individual	Living
bem	bem		Bemba (Zambia)	Individual	Living
ben	ben	bn	Bengali	Individual	Living
ber			Berber languages	Collective	
bho	bho		Bhojpuri	Individual	Living
bih		bh	Bihari languages	Collective	
bik	bik		Bikol	Macrolanguage	Living
bin	bin		Bini	Individual	Living
bis	bis	bi	Bislama	Individual	Living
bla	bla		Siksika	Individual	Living
bnt			Bantu languages	Collective	
bod / tib*	bod	bo	Tibetan	Individual	Living
bos	bos	bs	Bosnian	Individual	Living
bra	bra		Braj	Individual	Living
bre	bre	br	Breton	Individual	Living
btk			Batak languages	Collective	
bua	bua		Buriat	Macrolanguage	Living
bug	bug		Buginese	Individual	Living
bul	bul	bg	Bulgarian	Individual	Living
bur* / mya	mya	my	Burmese	Individual	Living
byn	byn		Bilin	Individual	Living
cad	cad		Caddo	Individual	Living
cai			Central American Indian languages	Collective	
car	car		Galibi Carib	Individual	Living
cat	cat	ca	Catalan	Individual	Living
cau			Caucasian languages	Collective	
cba			Chibchan languages	Collective	
ccn			North Caucasian languages	Collective	
ccs			South Caucasian languages	Collective	
cdc			Chadic languages	Collective	
cdd			Caddoan languages	Collective	
ceb	ceb		Cebuano	Individual	Living
cel			Celtic languages	Collective	
ces / cze*	ces	cs	Czech	Individual	Living
cha	cha	ch	Chamorro	Individual	Living
chb	chb		Chibcha	Individual	Extinct
che	che	ce	Chechen	Individual	Living
chg	chg		Chagatai	Individual	Extinct
chi* / zho	zho	zh	Chinese	Macrolanguage	Living
chk	chk		Chuukese	Individual	Living
chm	chm		Mari (Russia)	Macrolanguage	Living
chn	chn		Chinook jargon	Individual	Living
cho	cho		Choctaw	Individual	Living
chp	chp		Chipewyan	Individual	Living
chr	chr		Cherokee	Individual	Living
chu	chu	cu	Church Slavic	Individual	Ancient
chv	chv	cv	Chuvash	Individual	Living
chy	chy		Cheyenne	Individual	Living
cmc			Chamic languages	Collective	
cop	cop		Coptic	Individual	Extinct
cor	cor	kw	Cornish	Individual	Living
cos	cos	co	Corsican	Individual	Living
cpe			English based Creoles and pidgins	Collective	
cpf			French-Based Creoles and pidgins	Collective	
cpp			Portuguese-Based Creoles and pidgins	Collective	
cre	cre	cr	Cree	Macrolanguage	Living
crh	crh		Crimean Tatar	Individual	Living
crp			Creoles and pidgins	Collective	
csb	csb		Kashubian	Individual	Living
csu			Central Sudanic languages	Collective	
cus			Cushitic languages	Collective	
cym / wel*	cym	cy	Welsh	Individual	Living
cze* / ces	ces	cs	Czech	Individual	Living
dak	dak		Dakota	Individual	Living
dan	dan	da	Danish	Individual	Living
dar	dar		Dargwa	Individual	Living
day			Land Dayak languages	Collective	
del	del		Delaware	Macrolanguage	Living
den	den		Slave (Athapascan)	Macrolanguage	Living
deu / ger*	deu	de	German	Individual	Living
dgr	dgr		Dogrib	Individual	Living
din	din		Dinka	Macrolanguage	Living
div	div	dv	Dhivehi	Individual	Living
dmn			Mande languages	Collective	
doi	doi		Dogri (macrolanguage)	Macrolanguage	Living
dra			Dravidian languages	Collective	
dsb	dsb		Lower Sorbian	Individual	Living
dua	dua		Duala	Individual	Living
dum	dum		Middle Dutch (ca. 1050-1350)	Individual	Historical
dut* / nld	nld	nl	Dutch	Individual	Living
dyu	dyu		Dyula	Individual	Living
dzo	dzo	dz	Dzongkha	Individual	Living
efi	efi		Efik	Individual	Living
egx			Egyptian languages	Collective	
egy	egy		Egyptian (Ancient)	Individual	Ancient
eka	eka		Ekajuk	Individual	Living
ell / gre*	ell	el	Modern Greek (1453-)	Individual	Living
elx	elx		Elamite	Individual	Ancient
eng	eng	en	English	Individual	Living
enm	enm		Middle English (1100-1500)	Individual	Historical
epo	epo	eo	Esperanto	Individual	Constructed
est	est	et	Estonian	Macrolanguage	Living
esx			Eskimo-Aleut languages	Collective	
euq			Basque (family)	Collective	
eus / baq*	eus	eu	Basque	Individual	Living
ewe	ewe	ee	Ewe	Individual	Living
ewo	ewo		Ewondo	Individual	Living
fan	fan		Fang (Equatorial Guinea)	Individual	Living
fao	fao	fo	Faroese	Individual	Living
fas / per*	fas	fa	Persian	Macrolanguage	Living
fat	fat		Fanti	Individual	Living
fij	fij	fj	Fijian	Individual	Living
fil	fil		Filipino	Individual	Living
fin	fin	fi	Finnish	Individual	Living
fiu			Finno-Ugrian languages	Collective	
fon	fon		Fon	Individual	Living
fox			Formosan languages	Collective	
fra / fre*	fra	fr	French	Individual	Living
fre* / fra	fra	fr	French	Individual	Living
frm	frm		Middle French (ca. 1400-1600)	Individual	Historical
fro	fro		Old French (842-ca. 1400)	Individual	Historical
frr	frr		Northern Frisian	Individual	Living
frs	frs		Eastern Frisian	Individual	Living
fry	fry	fy	Western Frisian	Individual	Living
ful	ful	ff	Fulah	Macrolanguage	Living
fur	fur		Friulian	Individual	Living
gaa	gaa		Ga	Individual	Living
gay	gay		Gayo	Individual	Living
gba	gba		Gbaya (Central African Republic)	Macrolanguage	Living
gem			Germanic languages	Collective	
geo* / kat	kat	ka	Georgian	Individual	Living
ger* / deu	deu	de	German	Individual	Living
gez	gez		Geez	Individual	Ancient
gil	gil		Gilbertese	Individual	Living
gla	gla	gd	Scottish Gaelic	Individual	Living
gle	gle	ga	Irish	Individual	Living
glg	glg	gl	Galician	Individual	Living
glv	glv	gv	Manx	Individual	Living
gme			East Germanic languages	Collective	
gmh	gmh		Middle High German (ca. 1050-1500)	Individual	Historical
gmq			North Germanic languages	Collective	
gmw			West Germanic languages	Collective	
goh	goh		Old High German (ca. 750-1050)	Individual	Historical
gon	gon		Gondi	Macrolanguage	Living
gor	gor		Gorontalo	Individual	Living
got	got		Gothic	Individual	Ancient
grb	grb		Grebo	Macrolanguage	Living
grc	grc		Ancient Greek (to 1453)	Individual	Historical
gre* / ell	ell	el	Modern Greek (1453-)	Individual	Living
grk			Greek languages	Collective	
grn	grn	gn	Guarani	Macrolanguage	Living
gsw	gsw		Swiss German	Individual	Living
guj	guj	gu	Gujarati	Individual	Living
gwi	gwi		Gwichʼin	Individual	Living
hai	hai		Haida	Macrolanguage	Living
hat	hat	ht	Haitian	Individual	Living
hau	hau	ha	Hausa	Individual	Living
haw	haw		Hawaiian	Individual	Living
heb	heb	he	Hebrew	Individual	Living
her	her	hz	Herero	Individual	Living
hil	hil		Hiligaynon	Individual	Living
him			Himachali languages	Collective	
hin	hin	hi	Hindi	Individual	Living
hit	hit		Hittite	Individual	Ancient
hmn	hmn		Hmong	Macrolanguage	Living
hmo	hmo	ho	Hiri Motu	Individual	Living
hmx			Hmong-Mien languages	Collective	
hok			Hokan languages	Collective	
hrv	hrv	hr	Croatian	Individual	Living
hsb	hsb		Upper Sorbian	Individual	Living
hun	hun	hu	Hungarian	Individual	Living
hup	hup		Hupa	Individual	Living
hye / arm*	hye	hy	Armenian	Individual	Living
hyx			Armenian (family)	Collective	
iba	iba		Iban	Individual	Living
ibo	ibo	ig	Igbo	Individual	Living
ice* / isl	isl	is	Icelandic	Individual	Living
ido	ido	io	Ido	Individual	Constructed
iii	iii	ii	Sichuan Yi	Individual	Living
iir			Indo-Iranian languages	Collective	
ijo			Ijo languages	Collective	
iku	iku	iu	Inuktitut	Macrolanguage	Living
ile	ile	ie	Interlingue	Individual	Constructed
ilo	ilo		Iloko	Individual	Living
ina	ina	ia	Interlingua (International Auxiliary Language Association)	Individual	Constructed
inc			Indic languages	Collective	
ind	ind	id	Indonesian	Individual	Living
ine			Indo-European languages	Collective	
inh	inh		Ingush	Individual	Living
ipk	ipk	ik	Inupiaq	Macrolanguage	Living
ira			Iranian languages	Collective	
iro			Iroquoian languages	Collective	
isl / ice*	isl	is	Icelandic	Individual	Living
ita	ita	it	Italian	Individual	Living
itc			Italic languages	Collective	
jav	jav	jv	Javanese	Individual	Living
jbo	jbo		Lojban	Individual	Constructed
jpn	jpn	ja	Japanese	Individual	Living
jpr	jpr		Judeo-Persian	Individual	Living
jpx			Japanese (family)	Collective	
jrb	jrb		Judeo-Arabic	Macrolanguage	Living
kaa	kaa		Kara-Kalpak	Individual	Living
kab	kab		Kabyle	Individual	Living
kac	kac		Kachin	Individual	Living
kal	kal	kl	Kalaallisut	Individual	Living
kam	kam		Kamba (Kenya)	Individual	Living
kan	kan	kn	Kannada	Individual	Living
kar			Karen languages	Collective	
kas	kas	ks	Kashmiri	Individual	Living
kat / geo*	kat	ka	Georgian	Individual	Living
kau	kau	kr	Kanuri	Macrolanguage	Living
kaw	kaw		Kawi	Individual	Ancient
kaz	kaz	kk	Kazakh	Individual	Living
kbd	kbd		Kabardian	Individual	Living
kdo			Kordofanian languages	Collective	
kha	kha		Khasi	Individual	Living
khi			Khoisan languages	Collective	
khm	khm	km	Central Khmer	Individual	Living
kho	kho		Khotanese	Individual	Ancient
kik	kik	ki	Kikuyu	Individual	Living
kin	kin	rw	Kinyarwanda	Individual	Living
kir	kir	ky	Kirghiz	Individual	Living
kmb	kmb		Kimbundu	Individual	Living
kok	kok		Konkani (macrolanguage)	Macrolanguage	Living
kom	kom	kv	Komi	Macrolanguage	Living
kon	kon	kg	Kongo	Macrolanguage	Living
kor	kor	ko	Korean	Individual	Living
kos	kos		Kosraean	Individual	Living
kpe	kpe		Kpelle	Macrolanguage	Living
krc	krc		Karachay-Balkar	Individual	Living
krl	krl		Karelian	Individual	Living
kro			Kru languages	Collective	
kru	kru		Kurukh	Individual	Living
kua	kua	kj	Kuanyama	Individual	Living
kum	kum		Kumyk	Individual	Living
kur	kur	ku	Kurdish	Macrolanguage	Living
kut	kut		Kutenai	Individual	Living
lad	lad		Ladino	Individual	Living
lah	lah		Lahnda	Macrolanguage	Living
lam	lam		Lamba	Individual	Living
lao	lao	lo	Lao	Individual	Living
lat	lat	la	Latin	Individual	Ancient
lav	lav	lv	Latvian	Macrolanguage	Living
lez	lez		Lezghian	Individual	Living
lim	lim	li	Limburgan	Individual	Living
lin	lin	ln	Lingala	Individual	Living
lit	lit	lt	Lithuanian	Individual	Living
lol	lol		Mongo	Individual	Living
loz	loz		Lozi	Individual	Living
ltz	ltz	lb	Luxembourgish	Individual	Living
lua	lua		Luba-Lulua	Individual	Living
lub	lub	lu	Luba-Katanga	Individual	Living
lug	lug	lg	Ganda	Individual	Living
lui	lui		Luiseno	Individual	Living
lun	lun		Lunda	Individual	Living
luo	luo		Luo (Kenya and Tanzania)	Individual	Living
lus	lus		Lushai	Individual	Living
mac* / mkd	mkd	mk	Macedonian	Individual	Living
mad	mad		Madurese	Individual	Living
mag	mag		Magahi	Individual	Living
mah	mah	mh	Marshallese	Individual	Living
mai	mai		Maithili	Individual	Living
mak	mak		Makasar	Individual	Living
mal	mal	ml	Malayalam	Individual	Living
man	man		Mandingo	Macrolanguage	Living
mao* / mri	mri	mi	Maori	Individual	Living
map			Austronesian languages	Collective	
mar	mar	mr	Marathi	Individual	Living
mas	mas		Masai	Individual	Living
may* / msa	msa	ms	Malay (macrolanguage)	Macrolanguage	Living
mdf	mdf		Moksha	Individual	Living
mdr	mdr		Mandar	Individual	Living
men	men		Mende (Sierra Leone)	Individual	Living
mga	mga		Middle Irish (900-1200)	Individual	Historical
mic	mic		Mi'kmaq	Individual	Living
min	min		Minangkabau	Individual	Living
mis	mis		Uncoded languages	Special	
mkd / mac*	mkd	mk	Macedonian	Individual	Living
mkh			Mon-Khmer languages	Collective	
mlg	mlg	mg	Malagasy	Macrolanguage	Living
mlt	mlt	mt	Maltese	Individual	Living
mnc	mnc		Manchu	Individual	Living
mni	mni		Manipuri	Individual	Living
mno			Manobo languages	Collective	
moh	moh		Mohawk	Individual	Living
mon	mon	mn	Mongolian	Macrolanguage	Living
mos	mos		Mossi	Individual	Living
mri / mao*	mri	mi	Maori	Individual	Living
msa / may*	msa	ms	Malay (macrolanguage)	Macrolanguage	Living
mul	mul		Multiple languages	Special	
mun			Munda languages	Collective	
mus	mus		Creek	Individual	Living
mwl	mwl		Mirandese	Individual	Living
mwr	mwr		Marwari	Macrolanguage	Living
mya / bur*	mya	my	Burmese	Individual	Living
myn			Mayan languages	Collective	
myv	myv		Erzya	Individual	Living
nah			Nahuatl languages	Collective	
nai			North American Indian	Collective	
nap	nap		Neapolitan	Individual	Living
nau	nau	na	Nauru	Individual	Living
nav	nav	nv	Navajo	Individual	Living
nbl	nbl	nr	South Ndebele	Individual	Living
nde	nde	nd	North Ndebele	Individual	Living
ndo	ndo	ng	Ndonga	Individual	Living
nds	nds		Low German	Individual	Living
nep	nep	ne	Nepali (macrolanguage)	Macrolanguage	Living
new	new		Newari	Individual	Living
ngf			Trans-New Guinea languages	Collective	
nia	nia		Nias	Individual	Living
nic			Niger-Kordofanian languages	Collective	
niu	niu		Niuean	Individual	Living
nld / dut*	nld	nl	Dutch	Individual	Living
nno	nno	nn	Norwegian Nynorsk	Individual	Living
nob	nob	nb	Norwegian Bokmål	Individual	Living
nog	nog		Nogai	Individual	Living
non	non		Old Norse	Individual	Historical
nor	nor	no	Norwegian	Macrolanguage	Living
nqo	nqo		N'Ko	Individual	Living
nso	nso		Pedi	Individual	Living
nub			Nubian languages	Collective	
nwc	nwc		Classical Newari	Individual	Historical
nya	nya	ny	Nyanja	Individual	Living
nym	nym		Nyamwezi	Individual	Living
nyn	nyn		Nyankole	Individual	Living
nyo	nyo		Nyoro	Individual	Living
nzi	nzi		Nzima	Individual	Living
oci	oci	oc	Occitan (post 1500)	Individual	Living
oji	oji	oj	Ojibwa	Macrolanguage	Living
omq			Oto-Manguean languages	Collective	
omv			Omotic languages	Collective	
ori	ori	or	Oriya (macrolanguage)	Macrolanguage	Living
orm	orm	om	Oromo	Macrolanguage	Living
osa	osa		Osage	Individual	Living
oss	oss	os	Ossetian	Individual	Living
ota	ota		Ottoman Turkish (1500-1928)	Individual	Historical
oto			Otomian languages	Collective	
paa			Papuan languages	Collective	
pag	pag		Pangasinan	Individual	Living
pal	pal		Pahlavi	Individual	Ancient
pam	pam		Pampanga	Individual	Living
pan	pan	pa	Panjabi	Individual	Living
pap	pap		Papiamento	Individual	Living
pau	pau		Palauan	Individual	Living
peo	peo		Old Persian (ca. 600-400 B.C.)	Individual	Historical
per* / fas	fas	fa	Persian	Macrolanguage	Living
phi			Philippine languages	Collective	
phn	phn		Phoenician	Individual	Ancient
plf			Central Malayo-Polynesian languages	Collective	
pli	pli	pi	Pali	Individual	Ancient
pol	pol	pl	Polish	Individual	Living
pon	pon		Pohnpeian	Individual	Living
por	por	pt	Portuguese	Individual	Living
poz			Malayo-Polynesian languages	Collective	
pqe			Eastern Malayo-Polynesian languages	Collective	
pqw			Western Malayo-Polynesian languages	Collective	
pra			Prakrit languages	Collective	
pro	pro		Old Provençal (to 1500)	Individual	Historical
pus	pus	ps	Pushto	Macrolanguage	Living
qaa-qtz	qaa-qtz		Reserved for local use	Local	
que	que	qu	Quechua	Macrolanguage	Living
qwe			Quechuan (family)	Collective	
raj	raj		Rajasthani	Macrolanguage	Living
rap	rap		Rapanui	Individual	Living
rar	rar		Rarotongan	Individual	Living
roa			Romance languages	Collective	
roh	roh	rm	Romansh	Individual	Living
rom	rom		Romany	Macrolanguage	Living
ron / rum*	ron	ro	Romanian	Individual	Living
rum* / ron	ron	ro	Romanian	Individual	Living
run	run	rn	Rundi	Individual	Living
rup	rup		Macedo-Romanian	Individual	Living
rus	rus	ru	Russian	Individual	Living
sad	sad		Sandawe	Individual	Living
sag	sag	sg	Sango	Individual	Living
sah	sah		Yakut	Individual	Living
sai			South American Indian languages	Collective	
sal			Salishan languages	Collective	
sam	sam		Samaritan Aramaic	Individual	Extinct
san	san	sa	Sanskrit	Individual	Ancient
sas	sas		Sasak	Individual	Living
sat	sat		Santali	Individual	Living
scn	scn		Sicilian	Individual	Living
sco	sco		Scots	Individual	Living
sdv			Eastern Sudanic languages	Collective	
sel	sel		Selkup	Individual	Living
sem			Semitic languages	Collective	
sga	sga		Old Irish (to 900)	Individual	Historical
sgn			Sign languages	Collective	
shn	shn		Shan	Individual	Living
sid	sid		Sidamo	Individual	Living
sin	sin	si	Sinhala	Individual	Living
sio			Siouan languages	Collective	
sit			Sino-Tibetan languages	Collective	
sla			Slavic languages	Collective	
slo* / slk	slk	sk	Slovak	Individual	Living
slk / slo*	slk	sk	Slovak	Individual	Living
slv	slv	sl	Slovenian	Individual	Living
sma	sma		Southern Sami	Individual	Living
sme	sme	se	Northern Sami	Individual	Living
smi			Sami languages	Collective	
smj	smj		Lule Sami	Individual	Living
smn	smn		Inari Sami	Individual	Living
smo	smo	sm	Samoan	Individual	Living
sms	sms		Skolt Sami	Individual	Living
sna	sna	sn	Shona	Individual	Living
snd	snd	sd	Sindhi	Individual	Living
snk	snk		Soninke	Individual	Living
sog	sog		Sogdian	Individual	Ancient
som	som	so	Somali	Individual	Living
son			Songhai languages	Collective	
sot	sot	st	Southern Sotho	Individual	Living
spa	spa	es	Spanish	Individual	Living
sqi / alb*	sqi	sq	Albanian	Macrolanguage	Living
sqj			Albanian languages	Collective	
srd	srd	sc	Sardinian	Macrolanguage	Living
srn	srn		Sranan Tongo	Individual	Living
srp	srp	sr	Serbian	Individual	Living
srr	srr		Serer	Individual	Living
ssa			Nilo-Saharan languages	Collective	
ssw	ssw	ss	Swati	Individual	Living
suk	suk		Sukuma	Individual	Living
sun	sun	su	Sundanese	Individual	Living
sus	sus		Susu	Individual	Living
sux	sux		Sumerian	Individual	Ancient
swa	swa	sw	Swahili (macrolanguage)	Macrolanguage	Living
swe	swe	sv	Swedish	Individual	Living
syc	syc		Classical Syriac	Individual	Historical
syd			Samoyedic languages	Collective	
syr	syr		Syriac	Macrolanguage	Living
tah	tah	ty	Tahitian	Individual	Living
tai			Tai languages	Collective	
tam	tam	ta	Tamil	Individual	Living
tat	tat	tt	Tatar	Individual	Living
tbq			Tibeto-Burman languages	Collective	
tel	tel	te	Telugu	Individual	Living
tem	tem		Timne	Individual	Living
ter	ter		Tereno	Individual	Living
tet	tet		Tetum	Individual	Living
tgk	tgk	tg	Tajik	Individual	Living
tgl	tgl	tl	Tagalog	Individual	Living
tha	tha	th	Thai	Individual	Living
tib* / bod	bod	bo	Tibetan	Individual	Living
tig	tig		Tigre	Individual	Living
tir	tir	ti	Tigrinya	Individual	Living
tiv	tiv		Tiv	Individual	Living
tkl	tkl		Tokelau	Individual	Living
tlh	tlh		Klingon	Individual	Constructed
tli	tli		Tlingit	Individual	Living
tmh	tmh		Tamashek	Macrolanguage	Living
tog	tog		Tonga (Nyasa)	Individual	Living
ton	ton	to	Tonga (Tonga Islands)	Individual	Living
tpi	tpi		Tok Pisin	Individual	Living
trk			Turkic languages	Collective	
tsi	tsi		Tsimshian	Individual	Living
tsn	tsn	tn	Tswana	Individual	Living
tso	tso	ts	Tsonga	Individual	Living
tuk	tuk	tk	Turkmen	Individual	Living
tum	tum		Tumbuka	Individual	Living
tup			Tupi languages	Collective	
tur	tur	tr	Turkish	Individual	Living
tut			Altaic languages	Collective	
tuw			Tungus languages	Collective	
tvl	tvl		Tuvalu	Individual	Living
twi	twi	tw	Twi	Individual	Living
tyv	tyv		Tuvinian	Individual	Living
udm	udm		Udmurt	Individual	Living
uga	uga		Ugaritic	Individual	Ancient
uig	uig	ug	Uighur	Individual	Living
ukr	ukr	uk	Ukrainian	Individual	Living
umb	umb		Umbundu	Individual	Living
und	und		Undetermined	Special	
urd	urd	ur	Urdu	Individual	Living
urj			Uralic languages	Collective	
uzb	uzb	uz	Uzbek	Macrolanguage	Living
vai	vai		Vai	Individual	Living
ven	ven	ve	Venda	Individual	Living
vie	vie	vi	Vietnamese	Individual	Living
vol	vol	vo	Volapük	Individual	Constructed
vot	vot		Votic	Individual	Living
wak			Wakashan languages	Collective	
wal	wal		Wolaytta	Individual	Living
war	war		Waray (Philippines)	Individual	Living
was	was		Washo	Individual	Living
wel* / cym	cym	cy	Welsh	Individual	Living
wen			Sorbian languages	Collective	
wln	wln	wa	Walloon	Individual	Living
wol	wol	wo	Wolof	Individual	Living
xal	xal		Kalmyk	Individual	Living
xgn			Mongolian languages	Collective	
xho	xho	xh	Xhosa	Individual	Living
xnd			Na-Dene languages	Collective	
yao	yao		Yao	Individual	Living
yap	yap		Yapese	Individual	Living
yid	yid	yi	Yiddish	Macrolanguage	Living
yor	yor	yo	Yoruba	Individual	Living
ypk			Yupik languages	Collective	
zap	zap		Zapotec	Macrolanguage	Living
zbl	zbl		Blissymbols	Individual	Constructed
zen	zen		Zenaga	Individual	Living
zha	zha	za	Zhuang	Macrolanguage	Living
zho / chi*	zho	zh	Chinese	Macrolanguage	Living
zhx			Chinese (family)	Collective	
zle			East Slavic languages	Collective	
zls			South Slavic languages	Collective	
zlw			West Slavic languages	Collective	
znd			Zande languages	Collective	
zul	zul	zu	Zulu	Individual	Living
zun	zun		Zuni	Individual	Living
zxx	zxx		No linguistic content	Special	
zza	zza		Zaza	Macrolanguage	Living*/
}