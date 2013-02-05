/*
  Author: Christian Wulf
  E-Mail: chwchw@gmx.de

  Preprocessor semantics:
  - Chapter 9.5.1: "An implication of this is that #define and #undef directives in one
    source file have no effect on other source files in the same program."
*/

lexer grammar CSharp4PreProcessor;

options {
  language = Java;
}

//import Unicode;

@lexer::header {
package lang.csharp;

import java.util.Deque;
import java.util.LinkedList;
import java.util.Set;
import java.util.HashSet;
import java.util.Queue;
}

@lexer::members {
// if-then-else hierarchy controlling
protected Deque<Boolean> ifStack = new LinkedList<Boolean>();

// return value is only used for debugging purposes
protected boolean push(boolean expr) {
  /* if we are already within a block that should not be parsed due to current macro defs,
      do not parse the child if-section */
  if (!ifStack.peek()) {
    ifStack.push(false);
    return false;
  } else {
    ifStack.push(expr);
    return expr;
  }
}

protected boolean pop() {
  return ifStack.pop();
}

// static and dynamic macro definition controlling
protected Set<String> definedMacros = new HashSet<String>();

protected void define(final String conditionSymbol) {
  definedMacros.add(conditionSymbol);
}
  
protected void undefine(final String conditionSymbol) {
  definedMacros.remove(conditionSymbol);
}
  
protected boolean isDefined(final String conditionSymbol) {
  return definedMacros.contains(conditionSymbol);
}
  
// realizes emitation of multiple tokens within one single lexer rule
protected Queue<Token> tokens = new LinkedList<Token>();

protected void emit2(Token token, int type) {
  token.setType(type);
  emit(token);
}

@Override
public void emit(Token token) {
   state.token = token;
   tokens.add(token);
}

@Override
public Token nextToken() {
   super.nextToken();
   if (tokens.size() == 0) {
      return Token.EOF_TOKEN;
   }
   return tokens.remove();
}

// the following methods are only used for debug purposes
private List<String> errors = new LinkedList<String>();

@Override
public void displayRecognitionError(String[] tokenNames, RecognitionException e) {
    super.displayRecognitionError(tokenNames, e);
    String hdr = getErrorHeader(e);
    String msg = getErrorMessage(e, tokenNames);
    errors.add(hdr + " " + msg);
}

public List<String> getErrors() {
    return errors;
}

private void next(int n) {
  System.err.print("next: ");
  for (int i=1; i<=n; i++) {
    System.err.print(" | " + input.LA(i));
  }
  System.err.println();
}

}

//B.1.10 Pre_processing Directives
Pp_directive
  : (Pp_declaration
  | Pp_conditional
  | Pp_line
  | Pp_diagnostic
  | Pp_region
  | Pp_pragma
  ) {$channel=HIDDEN; }
  ;
fragment Pp_expression[Expression exprObj]
@init { Expression expr = new Expression(); }
  : WHITESPACE? Pp_or_expression[exprObj] WHITESPACE?
  ;
fragment Pp_or_expression[Expression exprObj]
@init { Expression expr = new Expression(); }
  : Pp_and_expression[expr] {exprObj.set(expr); } WHITESPACE?
    ('||' WHITESPACE? Pp_and_expression[expr] {exprObj.or(exprObj, expr);} )*
  ;
fragment Pp_and_expression[Expression exprObj]
@init { Expression expr = new Expression(); }
  : Pp_equality_expression[expr] {exprObj.set(expr);} WHITESPACE? 
    ('&&' WHITESPACE? Pp_equality_expression[expr] WHITESPACE? {exprObj.and(exprObj, expr);} )*
  ;
fragment Pp_equality_expression[Expression exprObj]
@init { Expression expr = new Expression(); }
  : Pp_unary_expression[expr] {exprObj.set(expr);} WHITESPACE?
    ( '==' WHITESPACE? Pp_unary_expression[expr] WHITESPACE? {exprObj.equal(exprObj, expr);}
    | '!=' WHITESPACE? Pp_unary_expression[expr] WHITESPACE? {exprObj.unequal(exprObj, expr);}
    )*
  ;
fragment Pp_unary_expression[Expression exprObj]
@init { Expression expr = new Expression(); }
  : Pp_primary_expression[expr] {exprObj.set(expr);}
  | '!' WHITESPACE? Pp_unary_expression[expr] {exprObj.not(expr);}
  ;
fragment Pp_primary_expression[Expression exprObj]
  : (TRUE) => TRUE {exprObj.set(true);}
  | (FALSE) => FALSE {exprObj.set(false);}
  | Conditional_symbol {exprObj.set(isDefined($Conditional_symbol.text)); }
  | '(' Pp_expression[exprObj] ')'
  ;
fragment Pp_declaration
  : WHITESPACE? SHARP WHITESPACE? 'define' WHITESPACE Conditional_symbol Pp_new_line
    {define($Conditional_symbol.text); }
  | WHITESPACE? SHARP WHITESPACE? 'undef' WHITESPACE Conditional_symbol Pp_new_line
    {undefine($Conditional_symbol.text); }
  ;
fragment Pp_new_line
  : WHITESPACE? SINGLE_LINE_COMMENT? NEW_LINE
  ;
// changed by chw
fragment Pp_conditional
  : Pp_if_section
  | Pp_elif_section
  | Pp_else_section
  | Pp_endif
  ;
fragment Pp_if_section
@init {Expression exprObj = new Expression();}
  : WHITESPACE? SHARP WHITESPACE? 'if' WHITESPACE e=Pp_expression[exprObj] Pp_new_line
      {boolean p=push(exprObj.isExpression());
        /*System.err.println("#if "+$e.text+" -> "+exprObj.isExpression());*/ }
  ;
fragment Pp_elif_section
@init {Expression exprObj = new Expression();}
  : WHITESPACE? SHARP WHITESPACE? 'elif' WHITESPACE Pp_expression[exprObj] Pp_new_line
  // if the if/elif-sections before has not been processed and expr is true
      {push(!pop() && exprObj.isExpression()); }
  ;
fragment Pp_else_section
  : WHITESPACE? SHARP WHITESPACE? 'else' Pp_new_line
  // if the if/elif-sections before has not been processed
      {push(!pop()); }
  ;
fragment Pp_endif
  : WHITESPACE? SHARP WHITESPACE? 'endif' Pp_new_line?
      {boolean p=pop(); /*System.err.println("endif: "+p); System.err.println("head: "+ifStack.peek());*/ }
  // ? is not conform, but now a file need not end with a newline
  ;
//'<Any Identifier_or_keyword Except True Or False>'
// WARNING: ignores exclusion
fragment Conditional_symbol
  : Identifier_or_keyword
  ;
fragment Pp_diagnostic
  : WHITESPACE? SHARP WHITESPACE? 'error' Pp_message
  | WHITESPACE? SHARP WHITESPACE? 'warning' Pp_message
  ;
fragment Pp_message
  : NEW_LINE
  | WHITESPACE Input_character* NEW_LINE
  ;
// changed by chw
fragment Pp_region
  : Pp_start_region
  | Pp_end_region
  ;
fragment Pp_start_region
  : WHITESPACE? SHARP WHITESPACE? 'region' Pp_message
  ;
fragment Pp_end_region
  : WHITESPACE? SHARP WHITESPACE? 'endregion' Pp_message?
  // ? is not conform, but now a file need not end with a newline
  ;
fragment Pp_line
  : WHITESPACE? SHARP WHITESPACE? 'line' WHITESPACE Line_indicator Pp_new_line
  ;
fragment Line_indicator
  : Decimal_digits (WHITESPACE File_name)?
  | 'default'
  | 'hidden'
  ;
fragment File_name
  : DOUBLE_QUOTE File_name_characters DOUBLE_QUOTE
  ;
fragment File_name_characters
  : File_name_character+
  ;
//'<Any input_character Except ">'
fragment File_name_character
  : ~( NEW_LINE_CHARACTER | DOUBLE_QUOTE )
  ;
// We use a more flexible pragma expression that also supports C# versions below 4.0
fragment Pp_pragma
  : WHITESPACE? SHARP WHITESPACE? 'pragma' Pp_pragma_text
  ;
fragment Pp_pragma_text
  : NEW_LINE?
  | WHITESPACE Input_characters? NEW_LINE?
  ;

// ----------------------------------------- Lexer ----------------------------------------

// A.1. Documentation Comments
SINGLE_LINE_DOC_COMMENT 
  : '///' Input_character* {$channel=HIDDEN; }
  ;
DELIMITED_DOC_COMMENT 
  : '/**' Delimited_comment_section* Asterisks '/' {$channel=HIDDEN; }
  ;

//B.1.1 Line Terminators
NEW_LINE 
  : ('\u000D' //'<Carriage Return Character (U+000D)>'
  | '\u000A' //'<Line Feed Character (U+000A)>'
  | '\u000D' '\u000A' //'<Carriage Return Character (U+000D) Followed By Line Feed Character (U+000A)>'
  | '\u0085' //<Next Line Character (U+0085)>'
  | '\u2028' //'<Line Separator Character (U+2028)>'
  | '\u2029' //'<Paragraph Separator Character (U+2029)>'
  ) {$channel=HIDDEN; }
  ;

//B.1.2 Comments
SINGLE_LINE_COMMENT 
  : '//' Input_character* {$channel=HIDDEN; }
  ;
fragment Input_characters
  : Input_character+
  ;
fragment Input_character 
  : ~NEW_LINE_CHARACTER //'<Any Unicode Character Except A NEW_LINE_CHARACTER>'
  ;
fragment NEW_LINE_CHARACTER 
  : '\u000D' //'<Carriage Return Character (U+000D)>'
  | '\u000A' //'<Line Feed Character (U+000A)>'
  | '\u0085' //'<Next Line Character (U+0085)>'
  | '\u2028' //'<Line Separator Character (U+2028)>'
  | '\u2029' //'<Paragraph Separator Character (U+2029)>'
  ;

DELIMITED_COMMENT 
  : '/*' Delimited_comment_section* Asterisks '/' {$channel=HIDDEN; }
  ;
fragment Delimited_comment_section 
  : '/'
  | Asterisks? Not_slash_or_asterisk
  ;
fragment Asterisks 
  : '*'+
  ;
//'<Any Unicode Character Except / Or *>'
fragment Not_slash_or_asterisk 
  : ~( '/' | '*' )
  ;

//B.1.3 White Space
WHITESPACE 
  : Whitespace_characters {$channel = HIDDEN;}
  ;

fragment Whitespace_characters 
  : Whitespace_character+
  ;

fragment Whitespace_character 
  : UNICODE_CLASS_ZS //'<Any Character With Unicode Class Zs>'
  | '\u0009' //'<Horizontal Tab Character (U+0009)>'
  | '\u000B' //'<Vertical Tab Character (U+000B)>'
  | '\u000C' //'<Form Feed Character (U+000C)>'
  ;

//B.1.5 Unicode Character Escape Sequences
fragment Unicode_escape_sequence 
  : '\\u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
  | '\\U' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
  ;

//B.1.7 Keywords
ABSTRACT : 'abstract';
AS : 'as';
BASE : 'base';
BOOL : 'bool';
BREAK : 'break';
BYTE : 'byte';
CASE : 'case';
CATCH : 'catch';
CHAR : 'char';
CHECKED : 'checked';
CLASS : 'class';
CONST : 'const';
CONTINUE : 'continue';
DECIMAL : 'decimal';
DEFAULT : 'default';
DELEGATE : 'delegate';
DO : 'do';
DOUBLE : 'double';
ELSE : 'else';
ENUM : 'enum';
EVENT : 'event';
EXPLICIT : 'explicit';
EXTERN : 'extern';
FALSE : 'false';
FINALLY : 'finally';
FIXED : 'fixed';
FLOAT : 'float';
FOR : 'for';
FOREACH : 'foreach';
GOTO : 'goto';
IF : 'if';
IMPLICIT : 'implicit';
IN : 'in';
INT : 'int';
INTERFACE : 'interface';
INTERNAL : 'internal';
IS : 'is';
LOCK : 'lock';
LONG : 'long';
NAMESPACE : 'namespace';
NEW : 'new';
NULL : 'null';
OBJECT : 'object';
OPERATOR : 'operator';
OUT : 'out';
OVERRIDE : 'override';
PARAMS : 'params';
PRIVATE : 'private';
PROTECTED : 'protected';
PUBLIC : 'public';
READONLY : 'readonly';
REF : 'ref';
RETURN : 'return';
SBYTE : 'sbyte';
SEALED : 'sealed';
SHORT : 'short';
SIZEOF : 'sizeof';
STACKALLOC : 'stackalloc';
STATIC : 'static';
STRING : 'string';
STRUCT : 'struct';
SWITCH : 'switch';
THIS : 'this';
THROW : 'throw';
TRUE : 'true';
TRY : 'try';
TYPEOF : 'typeof';
UINT : 'uint';
ULONG : 'ulong';
UNCHECKED : 'unchecked';
UNSAFE : 'unsafe';
USHORT : 'ushort';
USING : 'using';
VIRTUAL : 'virtual';
VOID : 'void';
VOLATILE : 'volatile';
WHILE : 'while';

//B.1.6 Identifiers
// must be defined after all keywords so the first branch (Available_identifier) does not match keywords 
IDENTIFIER
  : Available_identifier
  | '@' Identifier_or_keyword
  ;
//'<An Identifier_or_keyword That Is Not A Keyword>'
// WARNING: ignores exclusion
fragment Available_identifier 
  : Identifier_or_keyword
  ;
fragment Identifier_or_keyword 
  : Identifier_start_character Identifier_part_character*
  ;
fragment Identifier_start_character 
  : Letter_character
  | '_'
  ;
fragment Identifier_part_character 
  : Letter_character
  | Decimal_digit_character
  | Connecting_character
  | Combining_character
  | Formatting_character
  ;
//'<A Unicode Character Of Classes Lu, Ll, Lt, Lm, Lo, Or Nl>'
// WARNING: ignores Unicode_escape_sequence
fragment Letter_character 
  : UNICODE_CLASS_LU
  | UNICODE_CLASS_LL
  | UNICODE_CLASS_LT
  | UNICODE_CLASS_LM
  | UNICODE_CLASS_LO
  | UNICODE_CLASS_NL
//  | '<A Unicode_escape_sequence Representing A Character Of Classes Lu, Ll, Lt, Lm, Lo, Or Nl>'
  ;
//'<A Unicode Character Of Classes Mn Or Mc>'
// WARNING: ignores Unicode_escape_sequence
fragment Combining_character 
  : UNICODE_CLASS_MN
  | UNICODE_CLASS_MC
//  | '<A Unicode_escape_sequence Representing A Character Of Classes Mn Or Mc>'
  ;
//'<A Unicode Character Of The Class Nd>'
// WARNING: ignores Unicode_escape_sequence
fragment Decimal_digit_character 
  : UNICODE_CLASS_ND
//  | '<A Unicode_escape_sequence Representing A Character Of The Class Nd>'
  ;
//'<A Unicode Character Of The Class Pc>'
// WARNING: ignores Unicode_escape_sequence
fragment Connecting_character 
  : UNICODE_CLASS_PC
//  | '<A Unicode_escape_sequence Representing A Character Of The Class Pc>'
  ;
//'<A Unicode Character Of The Class Cf>'
// WARNING: ignores Unicode_escape_sequence
fragment Formatting_character 
  : UNICODE_CLASS_CF
//  | '<A Unicode_escape_sequence Representing A Character Of The Class Cf>'
  ;

//B.1.8 Literals

INTEGER_LITERAL 
  : Decimal_integer_literal
  | Hexadecimal_integer_literal
  ;
fragment Decimal_integer_literal 
  : Decimal_digits Integer_type_suffix?
  ;
fragment Decimal_digits 
  : DECIMAL_DIGIT+
  ;
fragment DECIMAL_DIGIT 
  : '0'..'9'
  ;
fragment Integer_type_suffix 
  : 'U'
  | 'u'
  | 'L'
  | 'l'
  | 'UL'
  | 'Ul'
  | 'uL'
  | 'ul'
  | 'LU'
  | 'Lu'
  | 'lU'
  | 'lu'
  ;
fragment Hexadecimal_integer_literal 
  : ('0x' | '0X') Hex_digits Integer_type_suffix?
  ;
fragment Hex_digits 
  : HEX_DIGIT+
  ;
fragment HEX_DIGIT 
  : '0'..'9'
  | 'A'..'F'
  | 'a'..'f'
  ;
// added by chw
// For the rare case where 0.ToString() etc is used.
// Explaination: 0.Equals() would be parsed as an invalid real (1. branch) causing a lexer error
LiteralAccess
  : il=INTEGER_LITERAL  {emit2($il, INTEGER_LITERAL);} 
    d=DOT               {emit2($d, DOT);}
    id=IDENTIFIER       {emit2($id, IDENTIFIER);}
  ;

REAL_LITERAL 
  : Decimal_digits DOT Decimal_digits Exponent_part? Real_type_suffix?
  | DOT Decimal_digits Exponent_part? Real_type_suffix?
  | Decimal_digits Exponent_part Real_type_suffix?
  | Decimal_digits Real_type_suffix
  ;
fragment Exponent_part 
  : ('e' | 'E') Sign? Decimal_digits
  ;
fragment Sign 
  : '+'
  | '-'
  ;
fragment Real_type_suffix 
  : 'F'
  | 'f'
  | 'D'
  | 'd'
  | 'M'
  | 'm'
  ;
CHARACTER_LITERAL 
  : QUOTE Character QUOTE
  ;
fragment Character 
  : Single_character
  | Simple_escape_sequence
  | Hexadecimal_escape_sequence
  | Unicode_escape_sequence
  ;
fragment Single_character 
  : ~(QUOTE
  | BACK_SLASH
  | NEW_LINE_CHARACTER) //'<Any Character Except \' (U+0027), \\ (U+005C), And NEW_LINE_CHARACTER>'
  ;
fragment Simple_escape_sequence 
  : '\\\''
  | '\\"'
  | DOUBLE_BACK_SLASH
  | '\\0'
  | '\\a'
  | '\\b'
  | '\\f'
  | '\\n'
  | '\\r'
  | '\\t'
  | '\\v'
  ;
fragment Hexadecimal_escape_sequence 
  : '\\x' HEX_DIGIT
  | '\\x' HEX_DIGIT HEX_DIGIT
  | '\\x' HEX_DIGIT HEX_DIGIT HEX_DIGIT
  | '\\x' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
  ;
STRING_LITERAL 
  : Regular_string_literal
  | Verbatim_string_literal
  ;
fragment Regular_string_literal 
  : DOUBLE_QUOTE Regular_string_literal_character* DOUBLE_QUOTE
  ;
fragment Regular_string_literal_character 
  : Single_regular_string_literal_character
  | Simple_escape_sequence
  | Hexadecimal_escape_sequence
  | Unicode_escape_sequence
  ;
//'<Any Character Except " (U+0022), \\ (U+005C), And NEW_LINE_CHARACTER>'
fragment Single_regular_string_literal_character 
  : ~( DOUBLE_QUOTE | BACK_SLASH | NEW_LINE_CHARACTER)
  ;
fragment Verbatim_string_literal 
  : '@' DOUBLE_QUOTE Verbatim_string_literal_character* DOUBLE_QUOTE
  ;
fragment Verbatim_string_literal_character 
  : Single_verbatim_string_literal_character
  | Quote_escape_sequence
  ;
fragment Single_verbatim_string_literal_character 
  : ~DOUBLE_QUOTE //<any Character Except ">
  ;
fragment Quote_escape_sequence 
  : DOUBLE_QUOTE DOUBLE_QUOTE
  ;

//B.1.9 Operators And Punctuators
OPEN_BRACE : '{';
CLOSE_BRACE : '}';
OPEN_BRACKET : '[';
CLOSE_BRACKET : ']';
OPEN_PARENS : '(';
CLOSE_PARENS : ')';
DOT : '.';
COMMA : ',';
COLON : ':';
SEMICOLON : ';';
PLUS : '+';
MINUS : '-';
STAR : '*';
DIV : '/';
PERCENT : '%';
AMP : '&';
BITWISE_OR : '|';
CARET : '^';
BANG : '!';
TILDE : '~';
ASSIGNMENT : '=';
LT : '<';
GT : '>';
INTERR : '?';
DOUBLE_COLON : '::';
OP_COALESCING : '??';
OP_INC : '++';
OP_DEC : '--';
OP_AND : '&&';
OP_OR : '||';
OP_PTR : '->';
OP_EQ : '==';
OP_NE : '!=';
OP_LE : '<=';
OP_GE : '>=';
OP_ADD_ASSIGNMENT : '+=';
OP_SUB_ASSIGNMENT : '-=';
OP_MULT_ASSIGNMENT : '*=';
OP_DIV_ASSIGNMENT : '/=';
OP_MOD_ASSIGNMENT : '%=';
OP_AND_ASSIGNMENT : '&=';
OP_OR_ASSIGNMENT : '|=';
OP_XOR_ASSIGNMENT : '^=';
OP_LEFT_SHIFT : '<<';
OP_LEFT_SHIFT_ASSIGNMENT : '<<=';

//B.1.10 Pre_processing Directives
// see above

// Custome Lexer rules
QUOTE :             '\'';
DOUBLE_QUOTE :      '"';
BACK_SLASH :        '\\';
DOUBLE_BACK_SLASH : '\\\\';
SHARP :             '#';

//// Unicode character classes
fragment UNICODE_CLASS_ZS
  : '\u0020' // SPACE
  | '\u00A0' // NO_BREAK SPACE
  | '\u1680' // OGHAM SPACE MARK
  | '\u180E' // MONGOLIAN VOWEL SEPARATOR
  | '\u2000' // EN QUAD
  | '\u2001' // EM QUAD
  | '\u2002' // EN SPACE
  | '\u2003' // EM SPACE
  | '\u2004' // THREE_PER_EM SPACE
  | '\u2005' // FOUR_PER_EM SPACE
  | '\u2006' // SIX_PER_EM SPACE
  | '\u2008' // PUNCTUATION SPACE
  | '\u2009' // THIN SPACE
  | '\u200A' // HAIR SPACE
  | '\u202F' // NARROW NO_BREAK SPACE
  | '\u3000' // IDEOGRAPHIC SPACE
  | '\u205F' // MEDIUM MATHEMATICAL SPACE
  ;

fragment UNICODE_CLASS_LU
  : '\u0041'..'\u005A' // LATIN CAPITAL LETTER A_Z
  | '\u00C0'..'\u00DE' // ACCENTED CAPITAL LETTERS
//  | { isUnicodeClass_Lu($text) }?
  ;

fragment UNICODE_CLASS_LL
  : '\u0061'..'\u007A' // LATIN SMALL LETTER a_z
  ;

fragment UNICODE_CLASS_LT
  : '\u01C5' // LATIN CAPITAL LETTER D WITH SMALL LETTER Z WITH CARON
  | '\u01C8' // LATIN CAPITAL LETTER L WITH SMALL LETTER J
  | '\u01CB' // LATIN CAPITAL LETTER N WITH SMALL LETTER J
  | '\u01F2' // LATIN CAPITAL LETTER D WITH SMALL LETTER Z
  ;

fragment UNICODE_CLASS_LM
  : '\u02B0'..'\u02EE' // MODIFIER LETTERS
  ;

fragment UNICODE_CLASS_LO
  : '\u01BB' // LATIN LETTER TWO WITH STROKE
  | '\u01C0' // LATIN LETTER DENTAL CLICK
  | '\u01C1' // LATIN LETTER LATERAL CLICK
  | '\u01C2' // LATIN LETTER ALVEOLAR CLICK
  | '\u01C3' // LATIN LETTER RETROFLEX CLICK
  | '\u0294' // LATIN LETTER GLOTTAL STOP
  ;

fragment UNICODE_CLASS_NL
  : '\u16EE' // RUNIC ARLAUG SYMBOL
  | '\u16EF' // RUNIC TVIMADUR SYMBOL
  | '\u16F0' // RUNIC BELGTHOR SYMBOL
  | '\u2160' // ROMAN NUMERAL ONE
  | '\u2161' // ROMAN NUMERAL TWO
  | '\u2162' // ROMAN NUMERAL THREE
  | '\u2163' // ROMAN NUMERAL FOUR
  | '\u2164' // ROMAN NUMERAL FIVE
  | '\u2165' // ROMAN NUMERAL SIX
  | '\u2166' // ROMAN NUMERAL SEVEN
  | '\u2167' // ROMAN NUMERAL EIGHT
  | '\u2168' // ROMAN NUMERAL NINE
  | '\u2169' // ROMAN NUMERAL TEN
  | '\u216A' // ROMAN NUMERAL ELEVEN
  | '\u216B' // ROMAN NUMERAL TWELVE
  | '\u216C' // ROMAN NUMERAL FIFTY
  | '\u216D' // ROMAN NUMERAL ONE HUNDRED
  | '\u216E' // ROMAN NUMERAL FIVE HUNDRED
  | '\u216F' // ROMAN NUMERAL ONE THOUSAND
  ;

fragment UNICODE_CLASS_MN
  : '\u0300' // COMBINING GRAVE ACCENT
  | '\u0301' // COMBINING ACUTE ACCENT
  | '\u0302' // COMBINING CIRCUMFLEX ACCENT
  | '\u0303' // COMBINING TILDE
  | '\u0304' // COMBINING MACRON
  | '\u0305' // COMBINING OVERLINE
  | '\u0306' // COMBINING BREVE
  | '\u0307' // COMBINING DOT ABOVE
  | '\u0308' // COMBINING DIAERESIS
  | '\u0309' // COMBINING HOOK ABOVE
  | '\u030A' // COMBINING RING ABOVE
  | '\u030B' // COMBINING DOUBLE ACUTE ACCENT
  | '\u030C' // COMBINING CARON
  | '\u030D' // COMBINING VERTICAL LINE ABOVE
  | '\u030E' // COMBINING DOUBLE VERTICAL LINE ABOVE
  | '\u030F' // COMBINING DOUBLE GRAVE ACCENT
  | '\u0310' // COMBINING CANDRABINDU
  ;

fragment UNICODE_CLASS_MC
  : '\u0903' // DEVANAGARI SIGN VISARGA
  | '\u093E' // DEVANAGARI VOWEL SIGN AA
  | '\u093F' // DEVANAGARI VOWEL SIGN I
  | '\u0940' // DEVANAGARI VOWEL SIGN II
  | '\u0949' // DEVANAGARI VOWEL SIGN CANDRA O
  | '\u094A' // DEVANAGARI VOWEL SIGN SHORT O
  | '\u094B' // DEVANAGARI VOWEL SIGN O
  | '\u094C' // DEVANAGARI VOWEL SIGN AU
  ;

fragment UNICODE_CLASS_CF
  : '\u00AD' // SOFT HYPHEN
  | '\u0600' // ARABIC NUMBER SIGN
  | '\u0601' // ARABIC SIGN SANAH
  | '\u0602' // ARABIC FOOTNOTE MARKER
  | '\u0603' // ARABIC SIGN SAFHA
  | '\u06DD' // ARABIC END OF AYAH
  ;

fragment UNICODE_CLASS_PC
  : '\u005F' // LOW LINE
  | '\u203F' // UNDERTIE
  | '\u2040' // CHARACTER TIE
  | '\u2054' // INVERTED UNDERTIE
  | '\uFE33' // PRESENTATION FORM FOR VERTICAL LOW LINE
  | '\uFE34' // PRESENTATION FORM FOR VERTICAL WAVY LOW LINE
  | '\uFE4D' // DASHED LOW LINE
  | '\uFE4E' // CENTRELINE LOW LINE
  | '\uFE4F' // WAVY LOW LINE
  | '\uFF3F' // FULLWIDTH LOW LINE
  ;

fragment UNICODE_CLASS_ND
  : '\u0030' // DIGIT ZERO
  | '\u0031' // DIGIT ONE
  | '\u0032' // DIGIT TWO
  | '\u0033' // DIGIT THREE
  | '\u0034' // DIGIT FOUR
  | '\u0035' // DIGIT FIVE
  | '\u0036' // DIGIT SIX
  | '\u0037' // DIGIT SEVEN
  | '\u0038' // DIGIT EIGHT
  | '\u0039' // DIGIT NINE
  ;

// the following preprocessor rules are only invoked by the extended Lexer class
// Hint: tokens need not to be skipped because they are matched within fragment rules
//   who in turn do not produce tokens by themselves
fragment SkiPped_section_part
  : WHITESPACE? SkiPped_characters? NEW_LINE
  | Pp_directive
  ;
fragment SkiPped_characters
  : Not_number_sign Input_character*
  ;
//'<Any Input_character Except #>'
// added Whitespace_character to solve warning in SkiPped_characters
fragment Not_number_sign
  : ~( Whitespace_character | NEW_LINE_CHARACTER | SHARP )
  ;
