grammar Metron2;

@header
{
using Metron2Configuration;
}


compileUnit returns [Configuration value]
	: { $value = new Configuration(); } first=line { $value.Merge($first.value); } (NL rest=line { $value.Merge($rest.value); })* NL? EOF
	;

line returns [Configuration value]
	: lineStart lineBody lineEnd { $value = $lineBody.value; } ws*
	| ws*
	;

lineStart returns [Variable<string> value]
	: pinOrVariable COMMA { $value = $pinOrVariable.value; }
	;

lineEnd
	: (COMMA crc?)?
	;

pinOrVariable returns [Variable<string> value]
	: pin { $value = new Variable<string> ($pin.text); }
	| variable { $value = new Variable<string> (null, $variable.value); }
	;

pin
	: letterOrNumber+
	;

variable returns [string value]
	: OPENBRACE letterOrNumber+ CLOSEBRACE { $value = $text.Substring(1, $text.Length - 2); }
	;

crc returns [string value]
	: hex4 { $value = $hex4.text; }
	;

lineBody returns [Configuration value]
	: D0 COMMA rtcLine { $value = $rtcLine.value; }
	| D1 COMMA phonebookLine { $value = $phonebookLine.value; }
	| D2 COMMA systemConfigurationLine { $value = new Configuration { SystemConfiguration = $systemConfigurationLine.value }; }
	| D3 COMMA channelLine { $value = $channelLine.value; }
	| D4 COMMA pinLine { $value = $pinLine.value; }
	| D5 COMMA resetUnitLine { $value = $resetUnitLine.value; }
	| D6 COMMA requestConfigLine { $value = $requestConfigLine.value; }
	| D7 COMMA gprsLine { $value = $gprsLine.value; }
	| D8 COMMA setupOutputLine { $value = $setupOutputLine.value; }
	| outputLine { $value = $outputLine.value; }
	;

rtcLine returns [Configuration value]
	: date=digits8 COMMA hour=digits2 COMMA minute=digits2
	;

phonebookLine returns [Configuration value]
	: entry1=notComma* (COMMA entry2=notComma* (COMMA entry3=notComma* (COMMA entry4=notComma*)?)?)?
	;

systemConfigurationLine returns [SystemConfiguration value]
	: systemName=name COMMA txHour=positiveInteger COMMA txMinute=positiveInteger COMMA variance=positiveInteger COMMA txInterval=positiveInteger COMMA wakeupInterval=positiveInteger COMMA tempEnabled=bool COMMA antenna=bool COMMA idle=idleValue COMMA readOnWake=bool COMMA log=positiveInteger COMMA batteryAlarm=positiveInteger (COMMA formatting=digit)? COMMA?
		{ $value = new SystemConfiguration { Name = $systemName.value, TxTime = MkTimeOfDayUtc($txHour.value, $txMinute.value), Variance = $variance.value, TxInterval = $txInterval.value + 1, WakeupInterval = $wakeupInterval.value, TemperatureEnabled = $tempEnabled.value, UseExternalAntenna = $antenna.value, Idle = $idle.value, ReadOnWake = $readOnWake.value, Log = $log.value, BatteryAlarmPercent = $batteryAlarm.value }; }
	;
	
idleValue returns [int value] 
   : digit { $value = $digit.value; } (digit { $value = $value * 10 + $digit.value; })?
   ;

channelLine returns [Configuration value]
	: channel=channelOrVariable COMMA channelSubcommand { ChannelConfiguration config = $channelSubcommand.value; $value = new Configuration(); ChannelConfigurationList channelList = new ChannelConfigurationList(); $value.ChannelsConfiguration.Add($channel.value, channelList); if (null != config) { config.Channel = $channel.value; channelList.Add(config); } }
	;

channelOrVariable returns [int value]
	: channel=digit { $value = $channel.value; }
	| variable { $value = 0; }
	;

channelSubcommand returns [ChannelConfiguration value]
	: A COMMA analogueChannel { $value = $analogueChannel.value; }
	| C COMMA lineariseChannel { $value = $lineariseChannel.value; }
	| D COMMA digitalChannel { $value = $digitalChannel.value; }
	| H COMMA highAlarmsChannel { $value = $highAlarmsChannel.value; }
	| L COMMA lowAlarmsChannel { $value = $lowAlarmsChannel.value; }
	| N COMMA notesChannel { $value = $notesChannel.value; }
	| P COMMA pulseChannel { $value = $pulseChannel.value; }
	| R COMMA rateOfChangeAlarmChannel { $value = $rateOfChangeAlarmChannel.value; }
	| S COMMA textOnAlarmChannel { $value = $textOnAlarmChannel.value; }
	| X disableChannel { $value = $disableChannel.value; }
	;

analogueChannel returns [AnalogueChannelConfiguration value]
	: channelName=nameOrBlank COMMA excitationVoltage=digit COMMA settleTime=positiveInteger COMMA engineeringUnits=name COMMA lowvalue=float COMMA highvalue=float COMMA loLoAlarm=float COMMA loAlarm=float COMMA hiAlarm=float COMMA hiHiAlarm=float COMMA hysteresis=positiveInteger COMMA calloutDelay=positiveInteger COMMA inputType=digit
		{ $value = new AnalogueChannelConfiguration() { Name = $channelName.value, ExcitationVoltage = ToExcitationVoltage($excitationVoltage.value), SettleTime = $settleTime.value, EngineeringUnits = $engineeringUnits.value, Zero = $lowvalue.value, Span = $highvalue.value, LoLoAlarm = $loLoAlarm.value, LoAlarm = $loAlarm.value, HiAlarm = $hiAlarm.value, Hysteresis = $hysteresis.value, CalloutDelay = $calloutDelay.value, InputType = ToInputType($inputType.value) }; }
	;

lineariseChannel returns [LineariseChannelConfiguration value]
	: first=floatOrBlank { $value = new LineariseChannelConfiguration(); $value.Values.Add($first.value); } (COMMA rest=floatOrBlank { $value.Values.Add($rest.value); })*  (COMMA last=float { $value.Values.Add($last.value); }) // This skips trailing commas and hence avoids lengthening the list with unnecessary items
	;

digitalChannel returns [ChannelConfiguration value]
	: channelName=name COMMA threshold=positiveInteger COMMA triggerOn=digit COMMA loHiMessage=name? COMMA hiLoMessage=name? COMMA callOutDelay=positiveInteger
	;

highAlarmsChannel returns [ChannelConfiguration value]
	: float (COMMA float?)*
	;

lowAlarmsChannel returns [ChannelConfiguration value]
	: float (COMMA float?)*
	;

notesChannel returns [ChannelConfiguration value]
	: notComma*
	;

pulseChannel returns [ChannelConfiguration value]
	: channelName=name COMMA positiveInteger COMMA positiveInteger COMMA engineeringUnits=name
	;

rateOfChangeAlarmChannel returns [ChannelConfiguration value]
	: positiveInteger COMMA positiveInteger COMMA positiveInteger
	;

textOnAlarmChannel returns [ChannelConfiguration value]
	: to1=boolOrBlank COMMA to2=boolOrBlank COMMA to3=boolOrBlank COMMA to4=boolOrBlank COMMA*
	;

disableChannel returns [DisableChannelConfiguration value]
	: COMMA* // Nothing else required
		{ $value = new DisableChannelConfiguration(); }
	;

pinLine returns [Configuration value]
	: pin
	;

resetUnitLine returns [Configuration value]
	: { $value = new Configuration { ResetUnit = true }; }
	;

requestConfigLine returns [Configuration value]
	: // Nothing more in here; return null, as there's no configuration to merge
	;

gprsLine returns [Configuration value]
	: gprsNumber=digit COMMA apn=notComma+ COMMA gprsUser=notComma* COMMA gprsPassword=notComma* COMMA hostAddress=notComma* COMMA port=positiveInteger COMMA hostPassword=notComma* COMMA protocol=positiveInteger
	;

setupOutputLine returns [Configuration value]
	: outputNumber=digit COMMA name COMMA alarmReferenceOrZero
	;

outputLine returns [Configuration value]
	: name COMMA onOffAuto
	;

nameOrBlank returns [string value]
	: notComma+ { $value = $text; }
	|
	;

name returns [string value]
	: notComma+ { $value = $text; }
	;

alarmReferenceOrZero
	: D0
	| input=digit (H | L) alarm=digit
	;

onOffAuto
	: O N
	| O F F
	| A U T O
	;

boolOrBlank returns [bool? value]
	: bool { $value = $bool.value; }
	|
	;

bool returns [bool value]
	: D0 { $value = false; }
	| D1 { $value = true; }
	;

digitOrBlank returns [int? value]
	: digit { $value = $digit.value; }
	|
	;

positiveIntegerOrBlank returns [int? value]
	: positiveInteger { $value = $positiveInteger.value; }
	|
	;

floatOrBlank returns [double? value]
	: float { $value = $float.value; }
	|
	;

positiveInteger returns [int value]
	: (digit { $value = $value * 10 + $digit.value; })+
	;

float returns [double value]
	: MINUS? digit* PERIOD digit+ { $value = double.Parse($text); }
	| MINUS? digit+ { $value = double.Parse($text); }
	;

hex4
	: hexDigit hexDigit hexDigit hexDigit
	;

digits2
	: digit digit
	;

digits4
	: digit digit digit digit
	;

digits8
	: digit digit digit digit digit digit digit digit
	;

hexDigit
	: digit
	| hexLetter
	;

letterOrNumber
	: digit
	| hexLetter
	| nonHexLetter
	;

notComma
	: digit
	| hexLetter
	| nonHexLetter
	| NONCOMMAPUNCTUATION
	| PERIOD
	| MINUS
	| SPACE
	| OPENBRACE
	| CLOSEBRACE
	;

digit returns [int value]
	: D0 { $value = 0; }
	| D1 { $value = 1; }
	| D2 { $value = 2; }
	| D3 { $value = 3; }
	| D4 { $value = 4; }
	| D5 { $value = 5; }
	| D6 { $value = 6; }
	| D7 { $value = 7; }
	| D8 { $value = 8; }
	| D9 { $value = 9; }
	;

hexLetter
	: A
	| B
	| C
	| D
	| E
	| F
	;

nonHexLetter
	: G
	| H
	| I
	| J
	| K
	| L
	| M
	| N
	| O
	| P
	| Q
	| R
	| S
	| T
	| U
	| V
	| W
	| X
	| Y
	| Z
	;

ws
	: SPACE
	| TAB
	;

// lexer

COMMA				: ',';
D0					: '0';
D1					: '1';
D2					: '2';
D3					: '3';
D4					: '4';
D5					: '5';
D6					: '6';
D7					: '7';
D8					: '8';
D9					: '9';
A					: 'A' | 'a';
B					: 'B' | 'b';
C					: 'C' | 'c';
D					: 'D' | 'd';
E					: 'E' | 'e';
F					: 'F' | 'f';
G					: 'G' | 'g';
H					: 'H' | 'h';
I					: 'I' | 'i';
J					: 'J' | 'j';
K					: 'K' | 'k';
L					: 'L' | 'l';
M					: 'M' | 'm';
N					: 'N' | 'n';
O					: 'O' | 'o';
P					: 'P' | 'p';
Q					: 'Q' | 'q';
R					: 'R' | 'r';
S					: 'S' | 's';
T					: 'T' | 't';
U					: 'U' | 'u';
V					: 'V' | 'v';
W					: 'W' | 'w';
X					: 'X' | 'x';
Y					: 'Y' | 'y';
Z					: 'Z' | 'z';
NL					: '\r'? '\n';
MINUS				: '-';
PERIOD				: '.';
NONCOMMAPUNCTUATION	: '!'..'+' | '-'..'/' | ':'..'@' | '['..'`' | '|' | '~';
OPENBRACE			: '{';
CLOSEBRACE			: '}';
SPACE				: ' ';
TAB					: '\t';