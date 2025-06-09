grammar TubeCube;

compileUnit returns [TubeCubeDump value]
	: NL* body (NL* errorMessage)? NL* EOF { $value = $body.value; }
	;

body returns [TubeCubeDump value]
	: headerPart { $value = new TubeCubeDump() { Header = $headerPart.value }; } (readingsPart { $value.Readings = $readingsPart.value; })? statusPart { $value.Status = $statusPart.value; } settingsPart endOfMessage // long form ("with headers")
	| readingsPart { $value = new TubeCubeDump() { Readings = $readingsPart.value }; } // short form ("without headers")
	;

errorMessage
	: crud+
	;

headerPart returns [TubeCubeHeader value]
	: attempt site tubeSerial headerDateTime { $value = new TubeCubeHeader($attempt.value, $site.value, $tubeSerial.value, $headerDateTime.value); } (headerSensors { $value.HeaderSensors = $headerSensors.value; })? blankLine? 
	;

attempt returns [int value]
	: ATTEMPT INT NL { $value = int.Parse($INT.text); }
	;

site returns [string value]
	: SITE COLON restofline NL { $value = $restofline.text; }
	;

restofline
	: crud*
	;
	
crud
	: identifier
	| INT
	| FLOAT
	| COLON
	| DASH
	| EQUALS
	| FULLSTOP
	| LPAREN
	| RPAREN
	| ADDR
	;

tubeSerial returns [string value]
	: TUBE sn COLON INT NL { $value = $INT.text; }
	;

headerDateTime returns [System.DateTime value]
	: TEXTDATE TIME NL { $value = MakeDate($TEXTDATE.text, $TIME.text); }
	;

headerSensors returns [List<HeaderSensor> value]
	: { $value = new List<HeaderSensor>(); } (headerSensor { $value.Add($headerSensor.value); })+
	;

headerSensor returns [HeaderSensor value]
	: SENSOR identifier /* "n." */ sensorNumber=INT TYPE COLON type=identifier sn COLON serialNo=INT NL { $value = new HeaderSensor($sensorNumber.text, $type.value, $serialNo.text); }
	;

sn
	: identifier /* "s/n" */
	;

readingsPart returns [TubeCubeReadings value]
	: headerRow (NL (readingRows { $value = new TubeCubeReadings() { Rows = $readingRows.value }; })?)? NL? 
	| noReadings { $value = new TubeCubeReadings() { Rows = new List<ReadingRow>() }; }
	;

noReadings
	: identifiers NL
	;

headerRow
	: { csvHeaderRow = new List<IColumnHeader>(); } (first=columnHeader { csvHeaderRow.Add($first.value); }) (COMMA other=columnHeader { csvHeaderRow.Add($other.value); })*
	;

columnHeader returns [IColumnHeader value]
	: SENSOR EN { $value = new SensorNumberColumnHeader(); }
	| RECORD EN { $value = new RecordNumberColumnHeader(); }
	| DATECOLUMN { $value = new DateColumnHeader(); }
	| TIMECOLUMN { $value = new TimeColumnHeader(); }
	| nameAndUnit { $value = $nameAndUnit.value; }
	;

nameAndUnit returns [IColumnHeader value]
	: name=identifiers { $value = new NameAndUnitColumnHeader() { Name = $name.value }; } (LPAREN unit=identifier RPAREN { ((NameAndUnitColumnHeader)$value).Unit = $unit.value; })?
	;

readingRows returns [List<ReadingRow> value]
	: { $value = new List<ReadingRow>(); } (first=readingRow { $value.Add($first.value); })(NL other=readingRow { $value.Add($other.value); })*
	;

readingRow returns [ReadingRow value]
	: { csvReadingRow = new List<IReading>(); } (first=reading { csvReadingRow.Add($first.value); }) (COMMA other=reading { csvReadingRow.Add($other.value); })* { $value = MakeReadingRow(); }
	;

reading returns [IReading value]
	: INT { $value = new DoubleReading(int.Parse($INT.text)); }
	| NUMERICDATE { $value = new DateReading($NUMERICDATE.text); }
	| TIME { $value = new TimeReading($TIME.text); }
	| FLOAT { $value = new DoubleReading(double.Parse($FLOAT.text)); }
	;

statusPart returns [TubeCubeStatus value]
	: statusHeader gsm temperature battery solarPart? firmware systemResets blankLine? { $value = new TubeCubeStatus() { GsmLevel = $gsm.value, Temperature = $temperature.value, BatteryLevel = $battery.value, FirmwareVersion = $firmware.value, SystemResets = $systemResets.value }; }
	;

statusHeader
	: DASH+ STATUS DASH+ NL*
	;

gsm returns [ValueAndUnit value]
	: gsmPrefix INT PERCENT crud* NL { $value = new ValueAndUnit() { Value = int.Parse($INT.text), Unit = $PERCENT.text }; }
	;

gsmPrefix
	: GSM LEVEL COLON
	| SIGNAL
	;

temperature returns [ValueAndUnit value]
	: TEMPERATURE COLON FLOAT identifier NL { $value = new ValueAndUnit() { Value = double.Parse($FLOAT.text), Unit = $identifier.value }; }
	;

battery returns [ValueAndUnit value]
	: BATTERY LEVEL COLON FLOAT identifier NL { $value = new ValueAndUnit() { Value = double.Parse($FLOAT.text), Unit = $identifier.value }; }
	;

solarPart
	: SOLAR PANEL CURRENT COLON NL (identifiers EQUALS FLOAT identifier NL)* restofline NL
	;

firmware returns [string value]
	: FIRMWARE VERSION COLON firmwareVersion NL { $value = $firmwareVersion.value; }
	;

firmwareVersion returns [string value]
	: FLOAT { $value = $FLOAT.text; }
	| IDENTIFIER { $value = $text; }
	;

systemResets returns [int value]
	: SYSTEM RESETS COLON INT NL { $value = int.Parse($INT.text); }
	;

settingsPart
	: settingsHeader wakeups blankLine* modems blankLine* loggings blankLine* probes blankLine*
	;

settingsHeader
	: DASH+ SETTINGS DASH+ NL*
	;

wakeups
	: WAKEUP PARAMETERS COLON NL wakeup*
	;

wakeup: DASH INT DAY COLON identifier COMMA START COLON TIME COMMA DURATION INT identifier COMMA ACTIVITY COLON QUOTEDSTRING NL+
	| DASH INT QUOTEDSTRING NL+
	;

modems
	: MODEM PARAMETERS COLON NL modemParameter*
	;

modemParameter
	: DASH restofline NL
	;

loggings
	: loggingHeader (setLoggingParameters | unsetLoggingParameters)
	;

loggingHeader
	: LOGGING PARAMETERS COLON NL
	;

setLoggingParameters
	: LOGGING PERIOD COLON TIME NL SAVE DATA EVERY INT identifier NL
	;

unsetLoggingParameters
	: QUOTEDSTRING NL
	;

probes
	: PROBES COLON NL probe*
	;

probe
	: DASH INT TYPE COLON identifier ADDR COLON INT NL
	;

blankLine
	: NL
	;

identifiers returns [string value]
	: first=identifier { $value = $first.value; } (rest=identifier { $value += " " + $rest.value; })*
	;

// Identifiers are allowed to be or contain keywords, which makes this a little messy.
identifier returns [string value]
	: IDENTIFIER { $value = $IDENTIFIER.text; }
	| ACTIVITY { $value = $ACTIVITY.text; }
	| ADDR { $value = $ADDR.text; }
	| ATTEMPT { $value = $ATTEMPT.text; }
	| BATTERY { $value = $BATTERY.text; }
	| CURRENT { $value = $CURRENT.text; }
	| DATA { $value = $DATA.text; }
	| DATECOLUMN { $value = $DATECOLUMN.text; }
	| DAY { $value = $DAY.text; }
	| DURATION { $value = $DURATION.text; }
	| EN { $value = $EN.text; }
	| END { $value = $END.text; }
	| ESS { $value = $ESS.text; }
	| EVERY { $value = $EVERY.text; }
	| FIRMWARE { $value = $FIRMWARE.text; }
	| GSM { $value = $GSM.text; }
	| LEVEL { $value = $LEVEL.text; }
	| LOGGING { $value = $LOGGING.text; }
	| MESSAGE { $value = $MESSAGE.text; }
	| MODEM { $value = $MODEM.text; }
	| OF { $value = $OF.text; }
	| PANEL { $value = $PANEL.text; }
	| PARAMETERS { $value = $PARAMETERS.text; }
	| PERIOD { $value = $PERIOD.text; }
	| PROBES { $value = $PROBES.text; }
	| RECORD { $value = $RECORD.text; }
	| RESETS { $value = $RESETS.text; }
	| SAVE { $value = $SAVE.text; }
	| SENSOR { $value = $SENSOR.text; }
	| SETTINGS { $value = $SETTINGS.text; }
	| SIGNAL { $value = $SIGNAL.text; }
	| SITE { $value = $SITE.text; }
	| SOLAR { $value = $SOLAR.text; }
	| START { $value = $START.text; }
	| STATUS { $value = $STATUS.text; }
	| SYSTEM { $value = $SYSTEM.text; }
	| TEMPERATURE { $value = $TEMPERATURE.text; }
	| TIMECOLUMN { $value = $TIMECOLUMN.text; }
	| TUBE { $value = $TUBE.text; }
	| TYPE { $value = $TYPE.text; }
	| VERSION { $value = $VERSION.text; }
	| WAKEUP { $value = $WAKEUP.text; }
	;

endOfMessage
	: DASH+ END OF MESSAGE DASH+ NL?
	;

// lexer

NUMERICDATE
	: DIGIT2 SLASH DIGIT2 SLASH DIGIT4
	;

TEXTDATE
	: DIGIT2 DASH MONTH3 DASH DIGIT4
	;

TIME
	: DIGIT2 ':' DIGIT2 (':' DIGIT2)?
	;

FLOAT
	: DASH? DIGIT+ FULLSTOP DIGIT+
	;

INT
	: DASH? DIGIT+
	;

ACTIVITY	: A C T I V I T Y;
ADDR		: A D D R;
ATTEMPT		: A T T E M P T;
BATTERY		: B A T T E R Y;
CURRENT		: C U R R E N T;
DATA		: D A T A;
DATECOLUMN	: D A T E;
DAY			: D A Y;
DURATION	: D U R A T I O N;
END			: E N D;
EVERY		: E V E R Y;
FIRMWARE	: F I R M W A R E;
GSM			: G S M;
LEVEL		: L E V E L;
LOGGING		: L O G G I N G;
MESSAGE		: M E S S A G E;
MODEM		: M O D E M;
OF			: O F;
PANEL		: P A N E L;
PARAMETERS	: P A R A M E T E R S;
PERIOD		: P E R I O D;
PROBES		: P R O B E S;
RECORD		: R E C O R D;
RESETS		: R E S E T S;
SAVE		: S A V E;
SENSOR		: S E N S O R;
SETTINGS	: S E T T I N G S;
SIGNAL		: S I G N A L;
SITE		: S I T E;
SOLAR		: S O L A R;
START		: S T A R T;
STATUS		: S T A T U S;
SYSTEM		: S Y S T E M;
TEMPERATURE	: T E M P E R A T U R E;
TIMECOLUMN	: T I M E;
TUBE		: T U B E;
TYPE		: T Y P E;
VERSION		: V E R S I O N;
WAKEUP		: W A K E U P;

COLON		: ':';
COMMA		: ',';
DASH		: '-';
EN			: N;
EQUALS		: '=';
ESS			: S;
LPAREN		: '(';
PERCENT		: '%';
FULLSTOP	: '.';
RPAREN		: ')';
SLASH		: '/';

NL	: '\r'? '\n'
	;

WS
	:	' '+ -> channel(HIDDEN)
	;

QUOTEDSTRING: '"' (~'"')* '"';

fragment DIGIT
	: '0'..'9'
	;

fragment DIGIT2
	: DIGIT DIGIT
	;

fragment DIGIT4
	: DIGIT DIGIT DIGIT DIGIT
	;

fragment MONTH3
	: J A N
	| F E B
	| M A R
	| A P R
	| M A Y
	| J U N
	| J U L
	| A U G
	| S E P T
	| S E P
	| O C T
	| N O V
	| D E C
	;

fragment A	:	'A'|'a';
fragment B	:	'B'|'b';
fragment C	:	'C'|'c';
fragment D	:	'D'|'d';
fragment E	:	'E'|'e';
fragment F	:	'F'|'f';
fragment G	:	'G'|'g';
fragment H	:	'H'|'h';
fragment I	:	'I'|'i';
fragment J	:	'J'|'j';
fragment K	:	'K'|'k';
fragment L	:	'L'|'l';
fragment M	:	'M'|'m';
fragment N	:	'N'|'n';
fragment O	:	'O'|'o';
fragment P	:	'P'|'p';
fragment Q	:	'Q'|'q';
fragment R	:	'R'|'r';
fragment S	:	'S'|'s';
fragment T	:	'T'|'t';
fragment U	:	'U'|'u';
fragment V	:	'V'|'v';
fragment W	:	'W'|'w';
fragment X	:	'X'|'x';
fragment Y	:	'Y'|'y';
fragment Z	:	'Z'|'z';

IDENTIFIER	: ~([-\r\n ,():./%=])~([-\r\n ,():%=])*;
