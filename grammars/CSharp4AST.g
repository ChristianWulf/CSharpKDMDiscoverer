parser grammar CSharp4AST;

options {
  language = Java;
  tokenVocab = CSharp4PreProcessor;
  output = AST;
}

tokens {
  QUALIFIED_IDENTIFIER;
  
  EXTERN_ALIAS_DIRECTIVES;
  USING_DIRECTIVES;
    USING_ALIAS_DIRECTIVE;
    USING_NAMESPACE_DIRECTIVE;
  NAMESPACE_MEMBER_DECLARATIONS;
  
  ATTRIBUTES;
  ATTRIBUTE_TARGET;
  ATTRIBUTE_LIST;
  ATTRIBUTE_NAME;
  ATTRIBUTE;
  POSITIONAL_ARGUMENT_LIST;
  // common AST nodes
  MODIFIERS;
  TYPE;
    RANK_SPECIFIER;
  BLOCK;
  NAMESPACE_OR_TYPE_NAME;
  QUALIFIED_ALIAS_MEMBER;
  NAMESPACE_OR_TYPE_PART;
  
  // class
  CLASS_MEMBER_DECLARATIONS;
    EXTENDS_OR_IMPLEMENTS;
    IMPLEMENTS;
    TYPE_PARAMETER_CONSTRAINTS_CLAUSES;
    TYPE_PARAMETER_CONSTRAINTS_CLAUSE;
  // interface
  INTERFACE_MEMBER_DECLARATIONS;
    VARIANT_TYPE_PARAMETERS;
    VARIANCE_ANNOTATION;
  // struct
    STRUCT_MEMBER_DECLARATIONS;
  // enum
  ENUM_EXTENDS;
  ENUM_MEMBER_DECLARATIONS;
  ENUM_MEMBER_DECLARATION;
  ENUM_MEMBER_INITIALIZER;
  
  CONSTRUCTOR_DECL;
  METHOD_DECL;
    MEMBER_NAME;
    FORMAL_PARAMETER_LIST;
    FIXED_PARAMETER;
    PARAMETER_MODIFIER;
    PARAMETER_ARRAY;
  FIELD_DECL;
    VARIABLE_DECLARATOR;
    VARIABLE_INITIALIZER;
  // const decl
    CONSTANT_DECLARATORS;
    CONSTANT_DECLARATOR;
    CONSTANT_INITIALIZER;
  INDEXER_DECL;
  PROPERTY_DECL;
    FIRST_OP;
    SECOND_OP;
  EVENT_VARS_DECL;
  EVENT_PROP_DECL;
  EVENT_INTERFACE_DECL;
  
  TYPE_ARGUMENT_LIST;
  TYPE_PARAMETERS;
  TYPE_PARAM;
  
  // statements
  EXPRESSION_STATEMENT;
  LABELED_STATEMENT;
  LOCAL_VARIABLE_DECLARATION;
  LOCAL_VARIABLE_DECLARATOR;
  LOCAL_VARIABLE_INITIALIZER;
  // if
    CONDITION;
    THEN;
  // for
    FOR_INITIALIZER;
    FOR_ITERATOR;
    
  // expressions
  LOOP_BODY;
  UNARY_EXPRESSION;
  CONDITIONAL_EXPRESSION;
  OP_RIGHT_SHIFT;
  OP_RIGHT_SHIFT_ASSIGNMENT;
  ASSIGNMENT_OPERATOR;
  SIMPLE_NAME;
  MEMBER_ACCESS;
  METHOD_INVOCATION;
  POST_INC;
  POST_DEC;
  ARGUMENT;
  ARGUMENT_VALUE;
  CAST_EXPRESSION;
  OBJECT_CREATION_EXPRESSION;
  BOOL_NOT;
  ARRAY_ACCESS;
}

@header {
package lang.csharp;

import java.util.LinkedList;
}

@members {
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
    System.err.print(" | " + input.LT(i).getType() + "=" + input.LT(i).getText());
  }
  System.err.println();
}

// scope variables
Object attrs;
Object members;

}

//B.2 Syntactic grammar

//B.2.1 Basic concepts
/** namespace_or_type_name */
namespace_name 
	: namespace_or_type_name
	;
type_name 
	: namespace_or_type_name
	;
/*
namespace_or_type_name 
	: IDENTIFIER type_argument_list?
	| namespace_or_type_name DOT IDENTIFIER type_argument_list?
	| qualified_alias_member
	;
*/
// added by chw
namespace_or_type_name
  : namespace_or_type_name2 -> ^(NAMESPACE_OR_TYPE_NAME  namespace_or_type_name2)
  ;
namespace_or_type_name2
  : ( IDENTIFIER type_argument_list_opt
    | qualified_alias_member
    ) namespace_part*
  ;
/* added by chw to solve a bug: IDENTIFIER of namespace_or_type_name2 is correctly parsed
    but not inserted into the AST */
namespace_part
  : DOT id2=IDENTIFIER type_argument_list_opt
    -> ^(NAMESPACE_OR_TYPE_PART $id2 type_argument_list_opt?)
  ;
/** represents type_argument_list? */
// added by chw
type_argument_list_opt
  : (type_argument_list) => type_argument_list  //-> ^(TYPE_ARGUMENT_LIST type_argument_list)
  | /* empty */ //-> ^(TYPE_ARGUMENT_LIST /* empty */)
  ;
//B.2.2 Types
/*
type 
	: value_type
	| reference_type
	| type_parameter
	| type_unsafe
	;
*/
type
  : type2
    -> ^(TYPE type2)
  ;
// added by chw
type2
  : base_type
    ( (INTERR) => INTERR                    
    | (rank_specifier) => rank_specifier   
    | STAR                                  
    )*
  ;
// added by chw
base_type
  : simple_type
  | class_type  // represents types: enum, class, interface, delegate, type_parameter
  | VOID STAR
  ;
/*
value_type 
	: struct_type
	| enum_type
	;
struct_type 
	: type_name
	| simple_type
	| nullable_type
	;
*/
/** primitive types */
simple_type 
	: numeric_type
	| BOOL
	;
numeric_type 
	: integral_type
	| floating_point_type
	| DECIMAL
	;
integral_type 
	: SBYTE
	| BYTE
	| SHORT
	| USHORT
	| INT
	| UINT
	| LONG
	| ULONG
	| CHAR
	;
floating_point_type 
	: FLOAT
	| DOUBLE
	;
nullable_type 
	: non_nullable_value_type INTERR
	;
/*
non_nullable_value_type 
  : type
  ;
*/
// type without INTERR; undocumented but VS checks for this constraint
non_nullable_value_type 
	: base_type
    ( (rank_specifier) => rank_specifier
    | STAR
    )*
	;
/* not used anymore
enum_type 
	: type_name
	;
*/
/*
reference_type 
	: class_type
	| interface_type
	| array_type
	| delegate_type
	;
*/
reference_type 
@init {boolean oneOrMore = false;}
  : ( simple_type {oneOrMore=true;}
    | class_type
    | VOID STAR {oneOrMore=true;}
  ) ((STAR | INTERR)* rank_specifier)*
    ({oneOrMore}? (STAR | INTERR)* rank_specifier)
  ;
/** type_name, OBJECT, "dynamic", STRING */
class_type 
	: type_name
	| OBJECT
	| dynamic_contextual_keyword
	| STRING
	;
/** type_name */
interface_type 
	: type_name
	;
/** type_name */
delegate_type 
	: type_name
	;
type_argument_list 
	: LT type_arguments GT
	  -> ^(TYPE_ARGUMENT_LIST type_arguments)
	;
type_arguments 
	: type_argument ( COMMA! type_argument)*
	;
type_argument 
	: type
	;
// added by chw
type_void
  : VOID -> ^(TYPE VOID)
  ;

//B.2.3 Variables
/** expression */
variable_reference 
	: expression
	;

//B.2.4 Expressions
argument_list 
	: argument ( COMMA! argument)*
	;
argument
	: argument_name? argument_value
	  -> ^(ARGUMENT argument_name? argument_value)
	;
argument_name
	: IDENTIFIER COLON!
	;
argument_value 
  : argument_value2 -> ^(ARGUMENT_VALUE argument_value2)
  ;
argument_value2
	: expression
	| REF variable_reference
	| OUT variable_reference
	;
/*
primary_expression 
	: primary_no_array_creation_expression
	| array_creation_expression
	;
*/
primary_expression 
  : (e=primary_expression_start -> $e)
    (bracket_expression -> ^(bracket_expression $primary_expression) )*
    ( ( member_access2 -> ^(MEMBER_ACCESS $primary_expression member_access2)
	    | method_invocation2 -> ^(METHOD_INVOCATION $primary_expression method_invocation2?)
	    | OP_INC -> ^(POST_INC $primary_expression)
	    | OP_DEC -> ^(POST_DEC $primary_expression)
	    | OP_PTR IDENTIFIER -> $primary_expression ^(OP_PTR  IDENTIFIER) 
	    )
	    (bracket_expression -> ^(bracket_expression  $primary_expression) )*
		)*
  ;
primary_expression_start
  : literal
  | simple_name
  | parenthesized_expression
  | predefined_type // member_access
  | qualified_alias_member  // member_access
  | this_access
  | base_access
  | NEW ( type ( object_creation_expression2^
               | object_or_collection_initializer
               | OPEN_BRACKET expression_list CLOSE_BRACKET rank_specifiers? array_initializer?
               | rank_specifiers array_initializer
               )
        | anonymous_object_initializer
        | rank_specifier array_initializer
        )
  | typeof_expression
  | checked_expression
  | unchecked_expression
  | default_value_expression
  | anonymous_method_expression
  | sizeof_expression
  ;
/*
bracket_expression
  : OPEN_BRACKET expression_list CLOSE_BRACKET
  | OPEN_BRACKET expression CLOSE_BRACKET
  ;
*/
bracket_expression
  : OPEN_BRACKET expression_list CLOSE_BRACKET
    -> ^(ARRAY_ACCESS expression_list)
  ;
/*
primary_no_array_creation_expression 
	: literal
	| simple_name
	| parenthesized_expression
	| member_access
	| invocation_expression
	| element_access
	| this_access
	| base_access
	| post_increment_expression
	| post_decrement_expression
	| object_creation_expression
	| delegate_creation_expression
	| anonymous_object_creation_expression
	| typeof_expression
	| checked_expression
	| unchecked_expression
	| default_value_expression
	| anonymous_method_expression
	| primary_no_array_creation_expression_unsafe
	;
*/
/*
primary_no_array_creation_expression 
	: ( literal
		| simple_name
		| parenthesized_expression
		| new_expression
		// member_access
    | predefined_type DOT IDENTIFIER type_argument_list?
    | (IDENTIFIER DOUBLE_COLON) => qualified_alias_member DOT IDENTIFIER
    | this_access
    | base_access
		| typeof_expression
		| checked_expression
		| unchecked_expression
		| default_value_expression
		| anonymous_method_expression
		// pointer_element_access
	  | sizeof_expression
	  ) ( DOT IDENTIFIER type_argument_list?
      | OPEN_PARENS argument_list? CLOSE_PARENS
      | OPEN_BRACKET expression_list CLOSE_BRACKET
      | OP_INC
      | OP_DEC
      | OP_PTR IDENTIFIER
      )*
	;
new_expression
  : NEW ( type ( OPEN_BRACKET expression_list CLOSE_BRACKET rank_specifiers? array_initializer? array_creation_tail
				       | rank_specifiers array_initializer array_creation_tail
				       | OPEN_PARENS argument_list? CLOSE_PARENS object_or_collection_initializer? // inludes delegate
				       | object_or_collection_initializer
				       )
	      | rank_specifier array_initializer array_creation_tail
	      | anonymous_object_initializer
	      )
  ;
array_creation_tail
    // member_access
  : DOT IDENTIFIER type_argument_list?
    // invocation_expression (but only one possibility)
  | OPEN_PARENS argument_list? CLOSE_PARENS
    // post inc
  | OP_INC
    // post dec
  | OP_DEC
    // pointer_member_access
  | OP_PTR IDENTIFIER
  ;
*/
/** IDENTIFIER type_argument_list? <br>
  (only used in primary_expression_start)
*/
simple_name 
	: IDENTIFIER type_argument_list_opt
	  -> ^(SIMPLE_NAME IDENTIFIER type_argument_list_opt?)
	;
/** OPEN_PARENS expression CLOSE_PARENS */
parenthesized_expression 
	: OPEN_PARENS! expression CLOSE_PARENS!
	;
/*
member_access 
	: primary_expression DOT IDENTIFIER type_argument_list?
	| predefined_type DOT IDENTIFIER type_argument_list?
	| qualified_alias_member DOT IDENTIFIER type_argument_list?
	;
*/
/** primary_expression */
member_access 
  : primary_expression
  ;
predefined_type 
	: BOOL
	| BYTE
	| CHAR
	| DECIMAL
	| DOUBLE
	| FLOAT
	| INT
	| LONG
	| OBJECT
	| SBYTE
	| SHORT
	| STRING
	| UINT
	| ULONG
	| USHORT
	;
/** primary_expression OPEN_PARENS argument_list? CLOSE_PARENS */
/* not used anymore; included in primary_expression
invocation_expression 
	: primary_expression OPEN_PARENS argument_list? CLOSE_PARENS
	;
*/
/*
element_access 
	: primary_no_array_creation_expression OPEN_BRACKET expression_list CLOSE_BRACKET
	;
*/
expression_list 
	: expression ( COMMA expression)*
	;
this_access 
	: THIS
	;
/** BASE and more */
base_access
	: BASE DOT! IDENTIFIER type_argument_list_opt
	| BASE OPEN_BRACKET expression_list CLOSE_BRACKET
	;
/* not used anymore; included in primary_expression
post_increment_expression 
	: primary_expression OP_INC
	;
post_decrement_expression 
	: primary_expression OP_DEC
	;
*/
/*
object_creation_expression 
	: NEW type OPEN_PARENS argument_list? CLOSE_PARENS object_or_collection_initializer?
	| NEW type object_or_collection_initializer
	;
*/
/** NEW type (OPEN_PARENS ... | OPEN_BRACE ...) */
object_creation_expression 
  : NEW type ( OPEN_PARENS argument_list? CLOSE_PARENS object_or_collection_initializer?
             | object_or_collection_initializer
             )
  ;
/** starts with OPEN_BRACE */
object_or_collection_initializer 
	: object_initializer
	| collection_initializer
	;
/*
object_initializer 
	: OPEN_BRACE member_initializer_list? CLOSE_BRACE
	| OPEN_BRACE member_initializer_list COMMA CLOSE_BRACE
	;
*/
/** starts with OPEN_BRACE */
object_initializer 
  : OPEN_BRACE CLOSE_BRACE
  | OPEN_BRACE member_initializer_list COMMA? CLOSE_BRACE
  ;
member_initializer_list 
	: member_initializer ( COMMA member_initializer)*
	;
member_initializer 
	: IDENTIFIER ASSIGNMENT initializer_value
	;
initializer_value 
	: expression
	| object_or_collection_initializer
	;
/*
collection_initializer 
	: OPEN_BRACE element_initializer_list CLOSE_BRACE
	| OPEN_BRACE element_initializer_list COMMA CLOSE_BRACE
	;
*/
/** starts with OPEN_BRACE */
collection_initializer 
  : OPEN_BRACE element_initializer_list COMMA? CLOSE_BRACE
  ;
element_initializer_list 
	: element_initializer ( COMMA element_initializer)*
	;
element_initializer 
	: non_assignment_expression
	| OPEN_BRACE expression_list CLOSE_BRACE
	;
/*
array_creation_expression 
	: NEW non_array_type OPEN_BRACKET expression_list CLOSE_BRACKET rank_specifiers? array_initializer?
	| NEW array_type array_initializer
	| NEW rank_specifier array_initializer
	;
*/
array_creation_expression 
  : NEW ( (array_type OPEN_BRACKET) => array_type array_initializer
        | non_array_type OPEN_BRACKET expression_list CLOSE_BRACKET rank_specifiers? array_initializer?
        | rank_specifier array_initializer
        )
  ;
/** NEW delegate_type OPEN_PARENS expression CLOSE_PARENS */
delegate_creation_expression 
	: NEW delegate_type OPEN_PARENS expression CLOSE_PARENS
	;
/** starts with NEW OPEN_BRACE */
anonymous_object_creation_expression 
	: NEW anonymous_object_initializer
	;
/*
anonymous_object_initializer 
	: OPEN_BRACE member_declarator_list? CLOSE_BRACE
	| OPEN_BRACE member_declarator_list COMMA CLOSE_BRACE
	;
*/
/** starts with OPEN_BRACE */
anonymous_object_initializer 
  : OPEN_BRACE CLOSE_BRACE
  | OPEN_BRACE member_declarator_list COMMA? CLOSE_BRACE
  ;
member_declarator_list 
	: member_declarator ( COMMA member_declarator)*
	;
/*
member_declarator 
	: simple_name
	| member_access
	| base_access
	| IDENTIFIER ASSIGNMENT expression
	;
*/
member_declarator 
  : primary_expression
  | IDENTIFIER ASSIGNMENT expression
  ;
typeof_expression 
	: TYPEOF OPEN_PARENS
	  ( (unbound_type_name) => unbound_type_name CLOSE_PARENS
	  | type CLOSE_PARENS
	  | VOID CLOSE_PARENS
	  )
	;
/*
unbound_type_name 
	: IDENTIFIER generic_dimension_specifier?
	| IDENTIFIER DOUBLE_COLON IDENTIFIER generic_dimension_specifier?
	| unbound_type_name DOT IDENTIFIER generic_dimension_specifier?
	;
*/
unbound_type_name 
  : IDENTIFIER ( generic_dimension_specifier?
               | DOUBLE_COLON IDENTIFIER generic_dimension_specifier?
               )
    (DOT IDENTIFIER generic_dimension_specifier?)*
  ;
generic_dimension_specifier 
	: LT commas? GT
	;
commas 
	: COMMA ( COMMA )*
	;
checked_expression 
	: CHECKED^ OPEN_PARENS! expression CLOSE_PARENS!
	;
unchecked_expression 
	: UNCHECKED^ OPEN_PARENS! expression CLOSE_PARENS!
	;
default_value_expression 
	: DEFAULT^ OPEN_PARENS! type CLOSE_PARENS!
	;
/*
unary_expression 
	: primary_expression
	| PLUS unary_expression
	| MINUS unary_expression
	| BANG unary_expression
	| TILDE unary_expression
	| pre_increment_expression
	| pre_decrement_expression
	| cast_expression
	| unary_expression_unsafe
	;
*/
unary_expression 
  : unary_expression2 -> ^(UNARY_EXPRESSION unary_expression2)
  ;
unary_expression2
	: (scan_for_cast_generic_precedence | OPEN_PARENS predefined_type) => cast_expression
	| primary_expression
	| PLUS unary_expression
	| MINUS unary_expression
	| BANG unary_expression  -> ^(BOOL_NOT unary_expression)
	| TILDE unary_expression
	| pre_increment_expression
	| pre_decrement_expression
	| unary_expression_unsafe
	;
// The sequence of tokens is correct grammar for a type, and the token immediately
// following the closing parentheses is the token TILDE, the token BANG, the token OPEN_PARENS,
// an IDENTIFIER, a literal, or any keyword except AS and IS.
scan_for_cast_generic_precedence
  : OPEN_PARENS type CLOSE_PARENS cast_disambiguation_token
  ;

// One of these tokens must follow a valid cast in an expression, in order to
// eliminate a grammar ambiguity.
cast_disambiguation_token
  : (TILDE | BANG | OPEN_PARENS | IDENTIFIER | literal | ABSTRACT | BASE | BOOL | BREAK | BYTE | CASE | CATCH
    | CHAR | CHECKED | CLASS | CONST | CONTINUE | DECIMAL | DEFAULT | DELEGATE | DO | DOUBLE | ELSE | ENUM
    | EVENT | EXPLICIT | EXTERN | FINALLY | FIXED | FLOAT | FOR | FOREACH | GOTO | IF | IMPLICIT | IN | INT
    | INTERFACE | INTERNAL | LOCK | LONG | NAMESPACE | NEW | OBJECT | OPERATOR | OUT | OVERRIDE | PARAMS
    | PRIVATE | PROTECTED | PUBLIC | READONLY | REF | RETURN | SBYTE | SEALED | SHORT | SIZEOF | STACKALLOC
    | STATIC | STRING | STRUCT | SWITCH | THIS | THROW | TRY | TYPEOF | UINT | ULONG | UNCHECKED | UNSAFE
    | USHORT | USING | VIRTUAL | VOID | VOLATILE | WHILE
    )
  ;
pre_increment_expression 
	: OP_INC unary_expression
	;
pre_decrement_expression 
	: OP_DEC unary_expression
	;
cast_expression 
	: OPEN_PARENS type CLOSE_PARENS unary_expression
	  -> ^(CAST_EXPRESSION type unary_expression)
	;

multiplicative_expression 
	: (e1=unary_expression -> $e1) 
	    ( STAR  e2=unary_expression -> ^(STAR $multiplicative_expression $e2)
	    | DIV  e2=unary_expression -> ^(DIV $multiplicative_expression $e2)
	    | PERCENT  e2=unary_expression -> ^(PERCENT $multiplicative_expression $e2)
	    )*
	;
additive_expression 
	: (e1=multiplicative_expression -> $e1)
	    ( PLUS  e2=multiplicative_expression -> ^(PLUS $additive_expression $e2)
	    | MINUS  e2=multiplicative_expression -> ^(MINUS $additive_expression $e2)
	    )*
	;
shift_expression 
	: (e1=additive_expression -> $e1)
	    ( OP_LEFT_SHIFT  e2=additive_expression -> ^(OP_LEFT_SHIFT $shift_expression $e2)
	    | right_shift  e2=additive_expression -> ^(right_shift $shift_expression $e2)
	    )*
	;
relational_expression 
	: (e1=shift_expression -> $e1)
	    ( LT e2=shift_expression -> ^(LT $relational_expression $e2)
      | GT e2=shift_expression -> ^(GT $relational_expression $e2)
      | OP_LE e2=shift_expression -> ^(OP_LE $relational_expression $e2)
      | OP_GE e2=shift_expression -> ^(OP_GE $relational_expression $e2)
      | IS e3=isType -> ^(IS $relational_expression $e3)
      | AS e4=type -> ^(AS $relational_expression $e4)
      )*
	;
// added by chw
isType
  : non_nullable_value_type ( (INTERR is_disambiguation_token) => INTERR)?
  ;
is_disambiguation_token
  : CLOSE_PARENS | OP_AND | OP_OR| INTERR
  ;
equality_expression 
  : (e1=relational_expression -> $e1)
      ( OP_EQ  e2=relational_expression -> ^(OP_EQ $equality_expression $e2)
      | OP_NE  e3=relational_expression -> ^(OP_NE $equality_expression $e3)
      )*
  ;
and_expression 
	: (e1=equality_expression -> $e1)
	    ( AMP e2=equality_expression -> ^(AMP $and_expression $e2)
	    )*
	;
exclusive_or_expression 
	: (e1=and_expression -> $e1)
	    ( CARET e2=and_expression -> ^(CARET $exclusive_or_expression $e2)
	    )*
	;
inclusive_or_expression 
	: (e1=exclusive_or_expression -> $e1)
	    ( BITWISE_OR e2=exclusive_or_expression -> ^(BITWISE_OR $inclusive_or_expression $e2)
	    )*
	;
conditional_and_expression 
	: (e1=inclusive_or_expression -> $e1)
	    ( OP_AND e2=inclusive_or_expression -> ^(OP_AND $conditional_and_expression $e2)
	    )*
	;
conditional_or_expression 
	: (e1=conditional_and_expression -> $e1)
	    ( OP_OR e2=conditional_and_expression -> ^(OP_OR $conditional_or_expression $e2)
	    )*
	;
/*
null_coalescing_expression 
	: conditional_or_expression
	| conditional_or_expression OP_COALESCING null_coalescing_expression
	;
*/
null_coalescing_expression 
  : conditional_or_expression (OP_COALESCING^ null_coalescing_expression)?
  ;
/*
conditional_expression 
	: null_coalescing_expression
	| null_coalescing_expression INTERR expression COLON expression
	;
*/
/** starts with unary_expression */
conditional_expression
  : (e1=null_coalescing_expression -> $e1)
    (INTERR expression COLON expression -> ^(CONDITIONAL_EXPRESSION ^(THEN expression) ^(ELSE expression)) )?
  ;

/** starts with OPEN_PARENS or IDENTIFIER */
lambda_expression 
	: anonymous_function_signature right_arrow anonymous_function_body
	;
/** starts with DELEGATE */
anonymous_method_expression 
	: DELEGATE^ explicit_anonymous_function_signature? block
	;
/*
anonymous_function_signature 
	: explicit_anonymous_function_signature
	| implicit_anonymous_function_signature
	;
*/
/** starts with OPEN_PARENS or IDENTIFIER */
anonymous_function_signature 
  : OPEN_PARENS CLOSE_PARENS
  | OPEN_PARENS explicit_anonymous_function_parameter_list CLOSE_PARENS
  | OPEN_PARENS implicit_anonymous_function_parameter_list CLOSE_PARENS
  | implicit_anonymous_function_parameter
  ;
explicit_anonymous_function_signature 
	: OPEN_PARENS explicit_anonymous_function_parameter_list? CLOSE_PARENS
	;
explicit_anonymous_function_parameter_list 
	: explicit_anonymous_function_parameter ( COMMA explicit_anonymous_function_parameter)*
	;
explicit_anonymous_function_parameter 
	: anonymous_function_parameter_modifier? type IDENTIFIER
	;
anonymous_function_parameter_modifier 
	: REF
	| OUT
	;
implicit_anonymous_function_signature 
	: OPEN_PARENS implicit_anonymous_function_parameter_list? CLOSE_PARENS
	| implicit_anonymous_function_parameter
	;
implicit_anonymous_function_parameter_list 
	: implicit_anonymous_function_parameter ( COMMA implicit_anonymous_function_parameter)*
	;
/** IDENTIFIER */
implicit_anonymous_function_parameter 
	: IDENTIFIER
	;
anonymous_function_body 
	: expression
	| block
	;
/** starts with from_contextual_keyword */
query_expression 
	: from_clause query_body
	;
from_clause 
	: from_contextual_keyword ((type IDENTIFIER IN) => type)? IDENTIFIER IN expression
	;
/*
query_body 
	: query_body_clauses? select_or_group_clause query_continuation?
	;
*/
query_body 
  : query_body_clauses? select_or_group_clause ((into_contextual_keyword) => query_continuation)?
  ;
query_body_clauses 
	: query_body_clause ( query_body_clause )*
	;
/*
query_body_clause 
	: from_clause
	| let_clause
	| where_clause
	| join_clause
	| join_into_clause
	| orderby_clause
	;
*/
query_body_clause 
  : from_clause
  | let_clause
  | where_clause
  | combined_join_clause
  | orderby_clause
  ;
let_clause 
	: let_contextual_keyword IDENTIFIER ASSIGNMENT expression
	;
where_clause 
	: where_contextual_keyword boolean_expression
	;
join_clause 
	: join_contextual_keyword type? IDENTIFIER IN expression on_contextual_keyword expression equals_contextual_keyword expression
	;
join_into_clause 
	: join_contextual_keyword type? IDENTIFIER IN expression on_contextual_keyword expression equals_contextual_keyword expression into_contextual_keyword IDENTIFIER
	;
// added by chw
combined_join_clause
  : join_contextual_keyword type? IDENTIFIER IN expression on_contextual_keyword expression equals_contextual_keyword expression (into_contextual_keyword IDENTIFIER)?
  ;
orderby_clause 
	: orderby_contextual_keyword orderings
	;
orderings 
	: ordering ( COMMA  ordering )*
	;
ordering 
	: expression ordering_direction?
	;
ordering_direction 
	: ascending_contextual_keyword
	| descending_contextual_keyword
	;
select_or_group_clause 
	: select_clause
	| group_clause
	;
select_clause 
	: select_contextual_keyword expression
	;
group_clause 
	: group_contextual_keyword expression by_contextual_keyword expression
	;
/** starts with into_contextual_keyword */
query_continuation 
	: into_contextual_keyword IDENTIFIER query_body
	;
/** starts with unary_expression */
assignment 
	: unary_expression assignment_operator^ expression
	;
assignment_operator
  : assignment_operator2  -> ^(ASSIGNMENT_OPERATOR assignment_operator2)
  ;
assignment_operator2
	: ASSIGNMENT
	| OP_ADD_ASSIGNMENT
	| OP_SUB_ASSIGNMENT
	| OP_MULT_ASSIGNMENT
	| OP_DIV_ASSIGNMENT
	| OP_MOD_ASSIGNMENT
	| OP_AND_ASSIGNMENT
	| OP_OR_ASSIGNMENT
	| OP_XOR_ASSIGNMENT
	| OP_LEFT_SHIFT_ASSIGNMENT
	| right_shift_assignment
	;
expression 
	: (assignment) => e1=assignment
	| e2=non_assignment_expression
	;
non_assignment_expression
	: (lambda_expression) => lambda_expression
	| (query_expression) => query_expression
	| c=conditional_expression
	;
constant_expression 
	: expression
	;
boolean_expression 
	: expression
	;

//B.2.5 Statements
statement 
	: (labeled_statement) => labeled_statement
	| (declaration_statement) => declaration_statement
	| embedded_statement
	;
embedded_statement 
	: block
	| empty_statement
	| expression_statement
	| selection_statement
	| iteration_statement
	| jump_statement
	| try_statement
	| checked_statement
	| unchecked_statement
	| lock_statement
	| using_statement
	| yield_statement
	| embedded_statement_unsafe
	;
/** starts with OPEN_BRACE */
block 
	: OPEN_BRACE statement_list? CLOSE_BRACE
	  -> ^(BLOCK statement_list?)
	;
statement_list 
	: statement+
	;
empty_statement 
	: SEMICOLON
	;
/** starts with IDENTIFIER COLON */
labeled_statement 
	: IDENTIFIER COLON statement
	  -> ^(LABELED_STATEMENT IDENTIFIER  statement)
	;
/** starts with type, VAR, or CONST */
declaration_statement 
	: local_variable_declaration SEMICOLON!
	| local_constant_declaration SEMICOLON!
	;
local_variable_declaration 
scope {
Object type;
}
	: t=local_variable_type!  {$local_variable_declaration::type = $t.tree;}  local_variable_declarators
	;
/*
local_variable_type 
  : type
  | 'var'
  ;
*/
local_variable_type 
	: type // includes 'var'
	;
/** starts with IDENTIFIER */
local_variable_declarators 
	: local_variable_declarator ( COMMA  local_variable_declarator )*
	  -> ^(LOCAL_VARIABLE_DECLARATOR  {$local_variable_declaration::type} local_variable_declarator )+
	;
/*
local_variable_declarator 
	: IDENTIFIER
	| IDENTIFIER ASSIGNMENT local_variable_initializer
	;
*/
local_variable_declarator 
  : IDENTIFIER (ASSIGNMENT! local_variable_initializer)?
  ;
local_variable_initializer
  : local_variable_initializer2 -> ^(LOCAL_VARIABLE_INITIALIZER local_variable_initializer2)
  ;
local_variable_initializer2
	: expression
	| array_initializer
	| local_variable_initializer_unsafe
	;
local_constant_declaration 
	: CONST! t=type! constant_declarators[t.tree]
	;
expression_statement
	: statement_expression SEMICOLON
	  -> ^(EXPRESSION_STATEMENT statement_expression)
	;
/*
statement_expression 
	: invocation_expression
	| object_creation_expression
	| assignment
	| post_increment_expression
	| post_decrement_expression
	| pre_increment_expression
	| pre_decrement_expression
	;
*/
// primary_expression includes invocation_expression,
//    object_creation_expression, post_increment_expression, and post_decrement_expression
statement_expression 
	: expression;
/** if or switch */
selection_statement 
	: if_statement
	| switch_statement
	;
/*
if_statement 
	: IF OPEN_PARENS boolean_expression CLOSE_PARENS embedded_statement
	| IF OPEN_PARENS boolean_expression CLOSE_PARENS embedded_statement ELSE embedded_statement
	;
*/
if_statement 
  : IF OPEN_PARENS be=boolean_expression CLOSE_PARENS thenStmt=embedded_statement
      ( (ELSE) => ELSE elseStmt=embedded_statement )?
    -> ^(IF ^(CONDITION $be) ^(THEN $thenStmt) ^(ELSE $elseStmt)? )
  ;
switch_statement 
	: SWITCH^ OPEN_PARENS expression CLOSE_PARENS switch_block
	;
switch_block 
	: OPEN_BRACE switch_sections? CLOSE_BRACE
	;
switch_sections 
	: switch_section ( switch_section )*
	;
switch_section 
	: switch_labels statement_list
	;
switch_labels 
	: switch_label ( switch_label )*
	;
switch_label 
	: CASE constant_expression COLON
	| DEFAULT COLON
	;
/** while, do, for, foreach */
iteration_statement 
	: while_statement
	| do_statement
	| for_statement
	| foreach_statement
	;
while_statement 
	: WHILE OPEN_PARENS boolean_expression CLOSE_PARENS embedded_statement
	  -> ^(WHILE ^(CONDITION boolean_expression) ^(LOOP_BODY embedded_statement) )
	;
do_statement 
	: DO embedded_statement WHILE OPEN_PARENS boolean_expression CLOSE_PARENS SEMICOLON
	  -> ^(DO ^(CONDITION boolean_expression) ^(LOOP_BODY embedded_statement) )
	;
for_statement 
	: FOR OPEN_PARENS for_initializer? SEMICOLON for_condition? SEMICOLON for_iterator? CLOSE_PARENS embedded_statement
	  -> ^(FOR ^(FOR_INITIALIZER for_initializer)? ^(CONDITION for_condition)? 
	          ^(FOR_ITERATOR  for_iterator)? ^(LOOP_BODY embedded_statement) )
	;
for_initializer 
	: (local_variable_declaration) => local_variable_declaration
	| statement_expression_list
	;
for_condition 
	: boolean_expression
	;
for_iterator 
	: statement_expression_list
	;
statement_expression_list 
	: statement_expression ( COMMA  statement_expression )*
	;
foreach_statement 
	: FOREACH OPEN_PARENS local_variable_type IDENTIFIER IN expression CLOSE_PARENS embedded_statement
	  -> ^(FOREACH local_variable_type IDENTIFIER  ^(IN expression)  embedded_statement)
	;
jump_statement 
	: break_statement
	| continue_statement
	| goto_statement
	| return_statement
	| throw_statement
	;
break_statement 
	: BREAK^ SEMICOLON!
	;
continue_statement 
	: CONTINUE^ SEMICOLON!
	;
goto_statement 
	: GOTO^ IDENTIFIER SEMICOLON
	| GOTO^ CASE constant_expression SEMICOLON
	| GOTO^ DEFAULT SEMICOLON
	;
return_statement 
	: RETURN^ expression? SEMICOLON!
	;
throw_statement 
	: THROW^ expression? SEMICOLON!
	;
/*
try_statement 
	: TRY block catch_clauses
	| TRY block finally_clause
	| TRY block catch_clauses finally_clause
	;
*/
try_statement 
  : TRY^ block catch_clauses? finally_clause?
  ;
/*
catch_clauses 
	: specific_catch_clauses general_catch_clause?
	| specific_catch_clauses? general_catch_clause
	;
*/
catch_clauses 
  : specific_catch_clauses general_catch_clause?
  | general_catch_clause
  ;
specific_catch_clauses 
	: specific_catch_clause ( specific_catch_clause )*
	;
specific_catch_clause 
	: CATCH^ OPEN_PARENS class_type IDENTIFIER? CLOSE_PARENS block
	;
general_catch_clause 
	: CATCH^ block
	;
finally_clause 
	: FINALLY^ block
	;
checked_statement 
	: CHECKED^ block
	;
unchecked_statement 
	: UNCHECKED^ block
	;
lock_statement 
	: LOCK^ OPEN_PARENS expression CLOSE_PARENS embedded_statement
	;
using_statement 
	: USING^ OPEN_PARENS resource_acquisition CLOSE_PARENS embedded_statement
	;
/*
resource_acquisition 
	: local_variable_declaration
	| expression
	;
*/
resource_acquisition 
	: (local_variable_declaration) => local_variable_declaration
	| expression
	;
yield_statement 
	: yield_contextual_keyword RETURN expression SEMICOLON
	| yield_contextual_keyword BREAK SEMICOLON
	;

// not used anymore; we use namespace_member_declaration+ directly

//B.2.6 Namespaces;
// entry point/ start rule
/*
compilation_unit 
	: extern_alias_directives? using_directives? namespace_member_declarations? global_attribute_sections? EOF
	;
*/
compilation_unit 
  : extern_alias_directives? using_directives?
    ( (global_attribute_section) => global_attribute_section )*
    namespace_member_declarations? EOF!
  ;
namespace_declaration 
	: NAMESPACE^ qualified_identifier namespace_body SEMICOLON!?
	;
qualified_identifier 
	: IDENTIFIER ( DOT  IDENTIFIER )*
	  -> ^(QUALIFIED_IDENTIFIER IDENTIFIER+)
	;
namespace_body 
	: OPEN_BRACE! extern_alias_directives? using_directives? namespace_member_declarations? CLOSE_BRACE!
	;
extern_alias_directives
	: extern_alias_directive+
	  -> ^(EXTERN_ALIAS_DIRECTIVES  extern_alias_directive+)
	;
extern_alias_directive
	: EXTERN^ alias_contextual_keyword! IDENTIFIER SEMICOLON!
	;
using_directives 
	: using_directive+
	  -> ^(USING_DIRECTIVES using_directive+)
	;
using_directive 
	: using_alias_directive
	| using_namespace_directive
	;
using_alias_directive 
	: USING IDENTIFIER ASSIGNMENT namespace_or_type_name SEMICOLON
	  -> ^(USING_ALIAS_DIRECTIVE  IDENTIFIER  namespace_or_type_name)
	;
using_namespace_directive 
	: USING namespace_name SEMICOLON
	  -> ^(USING_NAMESPACE_DIRECTIVE  namespace_name)
	;
namespace_member_declarations 
	: namespace_member_declaration+
	  -> ^(NAMESPACE_MEMBER_DECLARATIONS namespace_member_declaration+)
	;
namespace_member_declaration 
	: namespace_declaration
	| type_declaration
	;
/*
type_declaration 
	: class_declaration
	| struct_declaration
	| interface_declaration
	| enum_declaration
	| delegate_declaration
	;
*/
type_declaration 
  : attributes? all_member_modifiers?
    ( class_definition^
    | struct_definition^
    | interface_definition^
    | enum_definition^
    | delegate_definition^
    )
  ;
/** starts with IDENTIFIER DOUBLE_COLON */
qualified_alias_member 
	: id1=IDENTIFIER DOUBLE_COLON id2=IDENTIFIER type_argument_list_opt
	  -> ^(QUALIFIED_ALIAS_MEMBER $id1 $id2 type_argument_list_opt?)
	;

//B.2.7 Classes;
// not used anymore
class_declaration 
	: attributes? class_modifiers? partial_contextual_keyword? CLASS IDENTIFIER type_parameter_list?
	    class_base? type_parameter_constraints_clauses? class_body SEMICOLON?
	;
class_modifiers 
	: class_modifier ( class_modifier )*
	;
class_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| ABSTRACT
	| SEALED
	| STATIC
	| class_modifier_unsafe
	;
type_parameter_list 
	: LT! type_parameters GT!
	;
type_parameters 
	: attributed_type_parameter ( COMMA  attributed_type_parameter )*
	  -> ^(TYPE_PARAMETERS attributed_type_parameter+ )
	;
attributed_type_parameter
  : attributes? type_parameter  -> ^(TYPE_PARAM attributes? type_parameter)
  ;
/** IDENTIFIER */
type_parameter 
	: IDENTIFIER
	;
/*
class_base 
	: COLON class_type
	| COLON interface_type_list
	| COLON class_type COMMA interface_type_list
	;
*/
// class_type includes interface_type
class_base 
  : COLON class_type ( COMMA  interface_type )*
    -> ^(EXTENDS_OR_IMPLEMENTS  class_type  interface_type*)
  ;
interface_type_list 
	: interface_type ( COMMA  interface_type )*
	  -> ^(IMPLEMENTS interface_type+)
	;
type_parameter_constraints_clauses 
	: type_parameter_constraints_clause+
	  -> ^(TYPE_PARAMETER_CONSTRAINTS_CLAUSES type_parameter_constraints_clause+)
	;
type_parameter_constraints_clause 
	: where_contextual_keyword type_parameter COLON type_parameter_constraints
	  -> ^(TYPE_PARAMETER_CONSTRAINTS_CLAUSE  type_parameter  type_parameter_constraints)
	;
/*
type_parameter_constraints 
	: primary_constraint
	| secondary_constraints
	| constructor_constraint
	| primary_constraint COMMA secondary_constraints
	| primary_constraint COMMA constructor_constraint
	| secondary_constraints COMMA constructor_constraint
	| primary_constraint COMMA secondary_constraints COMMA constructor_constraint
	;
*/
type_parameter_constraints 
  : constructor_constraint
  | primary_constraint (COMMA secondary_constraints)? (COMMA constructor_constraint)?
  ;
primary_constraint 
	: class_type
	| CLASS
	| STRUCT
	;
/**
secondary_constraints 
	: interface_type
	| type_parameter
	| secondary_constraints COMMA interface_type
	| secondary_constraints COMMA type_parameter
	;
*/
// interface_type includes type_parameter
secondary_constraints
  : interface_type (COMMA interface_type)*
  ;
constructor_constraint 
	: NEW OPEN_PARENS CLOSE_PARENS
	;
class_body 
	: OPEN_BRACE! class_member_declarations? CLOSE_BRACE!
	;
class_member_declarations 
	: class_member_declaration+
	  -> ^(CLASS_MEMBER_DECLARATIONS class_member_declaration+ )
	;
/*
class_member_declaration 
	: constant_declaration
	| field_declaration
	| method_declaration
	| property_declaration
	| event_declaration
	| indexer_declaration
	| operator_declaration
	| constructor_declaration
	| destructor_declaration
	| static_constructor_declaration
	| type_declaration
	;
*/
class_member_declaration 
  : (attributes {attrs = $attributes.tree;})?
    (all_member_modifiers {members = $all_member_modifiers.tree;})?
	  ( common_member_declaration^
	  | destructor_definition^
	  )
  ;
// added by chw
// combines all available modifiers
all_member_modifiers
  : all_member_modifier+
    -> ^(MODIFIERS all_member_modifier+)
  ;
all_member_modifier
  : NEW
  | PUBLIC
  | PROTECTED
  | INTERNAL
  | PRIVATE
  | READONLY
  | VOLATILE
  | VIRTUAL
  | SEALED
  | OVERRIDE
  | ABSTRACT
  | STATIC
  | UNSAFE
  | EXTERN
  | partial_contextual_keyword
  ;
/** represents the intersection of struct_member_declaration and class_member_declaration */
/*common_member_declaration
  : constant_declaration
  | field_declaration
  | method_declaration
  | property_declaration
  | event_declaration
  | indexer_declaration
  | operator_declaration
  | constructor_declaration
  | static_constructor_declaration
  | type_declaration
  ;
*/
// added by chw
common_member_declaration
scope {
Object type;
}
  : constant_declaration2
  | typed_member_declaration
  | event_declaration2
  | conversion_operator_declarator^ operator_body
  // constructor_declaration and static_constructor_declaration
  | constructor_declaration2
  | type_void   method_declaration2^  // we use type_void instead of VOID to switch rules
  | class_definition
  | struct_definition
  | interface_definition
  | enum_definition
  | delegate_definition
  ;
// added by chw
typed_member_declaration
  : type {$common_member_declaration::type = $type.tree;}
    ( (interface_type DOT THIS) => interface_type DOT! indexer_declaration2^
    | (member_name type_parameter_list? OPEN_PARENS) => method_declaration2^
    | (member_name OPEN_BRACE) => property_declaration2^
    | indexer_declaration2^
    | operator_declaration2^
    | field_declaration2^
    )
  ;
/*
constant_declaration 
	: attributes? constant_modifiers? CONST type constant_declarators SEMICOLON
	;
constant_modifiers 
	: constant_modifier+
	;
constant_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	;
*/
constant_declarators[Object type]
	: constant_declarator[type] ( COMMA  constant_declarator[type] )*
	  -> ^(CONSTANT_DECLARATORS constant_declarator+ )
	;
constant_declarator[Object type]
	: IDENTIFIER ASSIGNMENT constant_expression
	  -> ^(CONSTANT_DECLARATOR {attrs} {members} {type} IDENTIFIER ^(CONSTANT_INITIALIZER constant_expression))
	;
/* not used anymore;
field_declaration 
	: attributes? field_modifiers? type variable_declarators SEMICOLON
	;
field_modifiers 
	: field_modifier ( field_modifier )*
	;
field_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| STATIC
	| READONLY
	| VOLATILE
	| field_modifier_unsafe
	;
*/
/** starts with IDENTIFIER */
variable_declarators
	: variable_declarator ( COMMA!  variable_declarator )*
	;
variable_declarator
  : variable_declarator2
    -> ^(VARIABLE_DECLARATOR {attrs} {members} {$common_member_declaration::type} variable_declarator2)
   ;
variable_declarator2
	: IDENTIFIER (ASSIGNMENT! variable_initializer)?
	;
variable_initializer
  : variable_initializer2 -> ^(VARIABLE_INITIALIZER  variable_initializer2)
  ;
variable_initializer2
	: expression
	| array_initializer
	;
method_declaration 
	: method_header method_body
	;
method_header 
	: attributes? method_modifiers? partial_contextual_keyword? return_type member_name type_parameter_list? OPEN_PARENS formal_parameter_list? CLOSE_PARENS type_parameter_constraints_clauses?
	;
method_modifiers 
	: method_modifier+
	;
method_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| STATIC
	| VIRTUAL
	| SEALED
	| OVERRIDE
	| ABSTRACT
	| EXTERN
	| method_modifier_unsafe
	;
/** type | VOID */
return_type 
	: type
	| VOID
	;
/*
member_name 
	: IDENTIFIER
	| interface_type DOT IDENTIFIER
	;
*/
/** interface_type */
member_name 
  : interface_type -> ^(MEMBER_NAME interface_type)
  ;
method_body 
	: block
	| SEMICOLON!
	;
/*
formal_parameter_list 
	: fixed_parameters
	| fixed_parameters COMMA parameter_array
	| parameter_array
	;
*/
formal_parameter_list 
  : (attributes? PARAMS) => parameter_array
    -> ^(FORMAL_PARAMETER_LIST parameter_array )
  | fixed_parameters ( (COMMA parameter_array) => COMMA parameter_array)?
    -> ^(FORMAL_PARAMETER_LIST  fixed_parameters  parameter_array?)
  ;
fixed_parameters 
  : fixed_parameter ( (COMMA fixed_parameter) => COMMA! fixed_parameter )*
  ;
/*
fixed_parameter 
  : attributes? parameter_modifier? type IDENTIFIER default_argument?
  ;
*/
// TODO add | '__arglist' etc.
fixed_parameter
  : attributes? parameter_modifier? type IDENTIFIER default_argument?
    -> ^(FIXED_PARAMETER attributes? parameter_modifier? type IDENTIFIER default_argument?)
  | arglist
    -> ^(FIXED_PARAMETER arglist)
  ;
default_argument 
	: ASSIGNMENT! expression
	;
parameter_modifier 
  : parameter_modifier2 -> ^(PARAMETER_MODIFIER parameter_modifier2)
  ;
parameter_modifier2
	: REF
	| OUT
	| THIS
	;
parameter_array 
	: attributes? PARAMS array_type IDENTIFIER
	  -> ^(PARAMETER_ARRAY attributes? array_type IDENTIFIER)
	;
property_declaration 
	: attributes? property_modifiers? type member_name OPEN_BRACE accessor_declarations CLOSE_BRACE
	;
property_modifiers 
	: property_modifier+
	;
property_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| STATIC
	| VIRTUAL
	| SEALED
	| OVERRIDE
	| ABSTRACT
	| EXTERN
	| property_modifier_unsafe
	;
/*
accessor_declarations 
	: get_accessor_declaration set_accessor_declaration?
	| set_accessor_declaration get_accessor_declaration?
	;
*/
accessor_declarations 
  : attrs=attributes?
    mods=accessor_modifier? 
    ( get_contextual_keyword accessor_body set_accessor_declaration?
    | set_contextual_keyword accessor_body get_accessor_declaration?
    )
  ;
get_accessor_declaration 
	: attributes? accessor_modifier? get_contextual_keyword accessor_body
	;
set_accessor_declaration 
	: attributes? accessor_modifier? set_contextual_keyword accessor_body
	;
accessor_modifier 
	: PROTECTED
	| INTERNAL
	| PRIVATE
	| PROTECTED INTERNAL
	| INTERNAL PROTECTED
	;
accessor_body 
	: block
	| SEMICOLON
	;
/*
event_declaration 
	: attributes? event_modifiers? EVENT type variable_declarators SEMICOLON
	| attributes? event_modifiers? EVENT type member_name OPEN_BRACE event_accessor_declarations CLOSE_BRACE
	;
*/
/* not used anymore due to inlining
event_declaration 
  : attributes? event_modifiers? EVENT type
    ( variable_declarators SEMICOLON
    | member_name OPEN_BRACE event_accessor_declarations CLOSE_BRACE
    )
  ;
*/
event_modifiers 
	: event_modifier+
	;
event_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| STATIC
	| VIRTUAL
	| SEALED
	| OVERRIDE
	| ABSTRACT
	| EXTERN
	| event_modifier_unsafe
	;
event_accessor_declarations 
	: attributes?
	  ( add_contextual_keyword block remove_accessor_declaration
	  | remove_contextual_keyword block add_accessor_declaration
	  )
	;
add_accessor_declaration 
	: attributes? add_contextual_keyword block
	;
remove_accessor_declaration 
	: attributes? remove_contextual_keyword block
	;
indexer_declaration 
	: attributes? indexer_modifiers? indexer_declarator OPEN_BRACE accessor_declarations CLOSE_BRACE
	;
indexer_modifiers 
	: indexer_modifier ( indexer_modifier )*
	;
indexer_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| VIRTUAL
	| SEALED
	| OVERRIDE
	| ABSTRACT
	| EXTERN
	| indexer_modifier_unsafe
	;
/*
indexer_declarator 
	: type THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET
	| type interface_type DOT THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET
	;
*/
indexer_declarator 
  : type (interface_type DOT)? THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET
  ;
operator_declaration 
	: attributes? operator_modifiers operator_declarator operator_body
	;
operator_modifiers 
	: operator_modifier ( operator_modifier )*
	;
operator_modifier 
	: PUBLIC
	| STATIC
	| EXTERN
	| operator_modifier_unsafe
	;
/*
operator_declarator 
	: unary_operator_declarator
	| binary_operator_declarator
	| conversion_operator_declarator
	;
*/
operator_declarator 
  : (unary_operator_declarator) => unary_operator_declarator
  | binary_operator_declarator
  | conversion_operator_declarator
  ;
unary_operator_declarator 
	: type OPERATOR overloadable_unary_operator OPEN_PARENS type IDENTIFIER CLOSE_PARENS
	;
overloadable_unary_operator 
	: PLUS
	| MINUS
	| BANG
	| TILDE
	| OP_INC
	| OP_DEC
	| TRUE
	| FALSE
	;
binary_operator_declarator 
	: type OPERATOR overloadable_binary_operator OPEN_PARENS type IDENTIFIER COMMA type IDENTIFIER CLOSE_PARENS
	;
overloadable_binary_operator 
	: PLUS
	| MINUS
	| STAR
	| DIV
	| PERCENT
	| AMP
	| BITWISE_OR
	| CARET
	| OP_LEFT_SHIFT
	| right_shift
	| OP_EQ
	| OP_NE
	| GT
	| LT
	| OP_GE
	| OP_LE
	;
// added by chw
/** includes the unary and the binary operators */
overloadable_operator
  : PLUS
  | MINUS
  | BANG
  | TILDE
  | OP_INC
  | OP_DEC
  | TRUE
  | FALSE
  | STAR
  | DIV
  | PERCENT
  | AMP
  | BITWISE_OR
  | CARET
  | OP_LEFT_SHIFT
  | right_shift
  | OP_EQ
  | OP_NE
  | GT
  | LT
  | OP_GE
  | OP_LE
  ;
/** starts with IMPLICIT or EXPLICIT */
conversion_operator_declarator
	: IMPLICIT^ OPERATOR type OPEN_PARENS type IDENTIFIER CLOSE_PARENS
	| EXPLICIT^ OPERATOR type OPEN_PARENS type IDENTIFIER CLOSE_PARENS
	;
operator_body 
	: block
	| SEMICOLON
	;
constructor_declaration 
	: attributes? constructor_modifiers? constructor_declarator constructor_body
	;
constructor_modifiers 
	: constructor_modifier+
	;
constructor_modifier 
	: PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| EXTERN
	| constructor_modifier_unsafe
	;
constructor_declarator 
	: IDENTIFIER OPEN_PARENS formal_parameter_list? CLOSE_PARENS constructor_initializer?
	;
constructor_initializer 
	: COLON BASE OPEN_PARENS argument_list? CLOSE_PARENS
	| COLON THIS OPEN_PARENS argument_list? CLOSE_PARENS
	;
constructor_body 
	: block
	| SEMICOLON
	;
static_constructor_declaration 
	: attributes? static_constructor_modifiers IDENTIFIER OPEN_PARENS CLOSE_PARENS static_constructor_body
	;
/*
static_constructor_modifiers 
	: EXTERN? STATIC
	| STATIC EXTERN?
	| static_constructor_modifiers_unsafe
	;
*/
static_constructor_modifiers 
  : static_constructor_modifiers_unsafe
  ;
static_constructor_body 
	: block
	| SEMICOLON
	;
/*
destructor_declaration 
	: attributes? EXTERN? TILDE IDENTIFIER OPEN_PARENS CLOSE_PARENS destructor_body
	| destructor_declaration_unsafe
	;
*/
destructor_declaration 
	: destructor_declaration_unsafe
	;
destructor_body 
	: block
	| SEMICOLON
	;
// added by chw
body
  : block
  | SEMICOLON
  ;

//B.2.8 Structs
/** is not used anymore */
struct_declaration 
	: attributes? struct_modifiers? partial_contextual_keyword? 
	  STRUCT IDENTIFIER type_parameter_list? struct_interfaces? type_parameter_constraints_clauses? struct_body SEMICOLON?
	;
struct_modifiers 
	: struct_modifier ( struct_modifier )*
	;
struct_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| struct_modifier_unsafe
	;
struct_interfaces 
	: COLON! interface_type_list
	;
struct_body 
	: OPEN_BRACE! struct_member_declarations? CLOSE_BRACE!
	;
struct_member_declarations 
	: struct_member_declaration ( struct_member_declaration )*
	  -> ^(STRUCT_MEMBER_DECLARATIONS struct_member_declaration+)
	;
/*
struct_member_declaration 
	: constant_declaration
	| field_declaration
	| method_declaration
	| property_declaration
	| event_declaration
	| indexer_declaration
	| operator_declaration
	| constructor_declaration
	| static_constructor_declaration
	| type_declaration
	| struct_member_declaration_unsafe
	;
*/
struct_member_declaration
	: (attributes {attrs = $attributes.tree;})?
	  (all_member_modifiers {members = $all_member_modifiers.tree;})?
		( common_member_declaration^
		| FIXED^ buffer_element_type fixed_size_buffer_declarators SEMICOLON!
		)
	;
//B.2.9 Arrays
/*
array_type 
	: non_array_type rank_specifiers
	;
*/
/** non_array_type rank_specifiers */
array_type 
  : array_type2 -> ^(TYPE array_type2)
  ;
array_type2
	: base_type ((STAR | INTERR)* rank_specifier)+
	;
/*
non_array_type 
	: type
	;
*/
/** type */
non_array_type 
  : non_array_type2 -> ^(TYPE non_array_type2)
  ;
non_array_type2 
	: base_type (rank_specifier | INTERR | STAR)*
	;
/*
rank_specifiers 
	: rank_specifier ( rank_specifier )*
	;
*/
/** starts with OPEN_BRACKET */
rank_specifiers 
  : rank_specifier+
  ;
/** OPEN_BRACKET dim_separators? CLOSE_BRACKET */
rank_specifier 
	: OPEN_BRACKET dim_separators? CLOSE_BRACKET
	  -> ^(RANK_SPECIFIER dim_separators?)
	;
dim_separators 
	: COMMA+
	;
/*
array_initializer 
	: OPEN_BRACE variable_initializer_list? CLOSE_BRACE
	| OPEN_BRACE variable_initializer_list COMMA CLOSE_BRACE
	;
*/
/** starts with OPEN_BRACE */
array_initializer 
  : OPEN_BRACE CLOSE_BRACE
  | OPEN_BRACE variable_initializer_list COMMA? CLOSE_BRACE
  ;
variable_initializer_list 
	: variable_initializer ( COMMA  variable_initializer )*
	;
//B.2.10 Interfaces
interface_declaration 
	: attributes? interface_modifiers? partial_contextual_keyword? INTERFACE IDENTIFIER variant_type_parameter_list? interface_base? type_parameter_constraints_clauses? interface_body SEMICOLON?
	;
interface_modifiers 
	: interface_modifier ( interface_modifier )*
	;
interface_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| interface_modifier_unsafe
	;
variant_type_parameter_list 
	: LT! variant_type_parameters GT!
	;
variant_type_parameters 
	: attributed_variance_type_parameter ( COMMA attributed_variance_type_parameter )*
	  -> ^(VARIANT_TYPE_PARAMETERS attributed_variance_type_parameter+ )
	;
// added by chw for modularization purposes
attributed_variance_type_parameter
  : attributes? variance_annotation? type_parameter
  ;
variance_annotation 
	: IN   -> ^(VARIANCE_ANNOTATION IN)
	| OUT  -> ^(VARIANCE_ANNOTATION OUT)
	;
interface_base 
	: COLON! interface_type_list
	;
interface_body 
	: OPEN_BRACE! interface_member_declarations? CLOSE_BRACE!
	;
interface_member_declarations 
	: interface_member_declaration+
	  -> ^(INTERFACE_MEMBER_DECLARATIONS interface_member_declaration+ )
	;
/*
interface_member_declaration 
	: interface_method_declaration
	| interface_property_declaration
	| interface_event_declaration
	| interface_indexer_declaration
	;
*/
interface_member_declaration 
  : attributes? NEW?
    ( type
      ( interface_method_declaration2^
      | interface_property_declaration2^
      | interface_indexer_declaration2^
      )
    | type_void interface_method_declaration2^
    | interface_event_declaration2^
    )
  ;
interface_method_declaration 
	: attributes? NEW? return_type IDENTIFIER type_parameter_list? OPEN_PARENS formal_parameter_list? CLOSE_PARENS type_parameter_constraints_clauses? SEMICOLON
	;
interface_property_declaration 
	: attributes? NEW? type IDENTIFIER OPEN_BRACE interface_accessors CLOSE_BRACE
	;
/*
interface_accessors 
	: attributes? get_contextual_keyword SEMICOLON
	| attributes? set_contextual_keyword SEMICOLON
	| attributes? get_contextual_keyword SEMICOLON attributes? set_contextual_keyword SEMICOLON
	| attributes? set_contextual_keyword SEMICOLON attributes? get_contextual_keyword SEMICOLON
	;
*/
interface_accessors 
  : attributes?
    ( get_contextual_keyword SEMICOLON! (attributes? set_contextual_keyword SEMICOLON!)?
    | set_contextual_keyword SEMICOLON! (attributes? get_contextual_keyword SEMICOLON!)?
    )
  ;
interface_event_declaration 
	: attributes? NEW? EVENT type IDENTIFIER SEMICOLON
	;
interface_indexer_declaration 
	: attributes? NEW? type THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET OPEN_BRACE interface_accessors CLOSE_BRACE
	;


//B.2.11 Enums
enum_declaration 
	: attributes? enum_modifiers? ENUM IDENTIFIER enum_base? enum_body SEMICOLON?
	;
enum_base 
	: COLON integral_type
	  -> ^(ENUM_EXTENDS integral_type)
	;
/*
enum_body 
	: OPEN_BRACE enum_member_declarations? CLOSE_BRACE
	| OPEN_BRACE enum_member_declarations COMMA CLOSE_BRACE
	;
*/
enum_body 
  : OPEN_BRACE! CLOSE_BRACE!
  | OPEN_BRACE! enum_member_declarations COMMA!? CLOSE_BRACE!
  ;
enum_modifiers 
	: enum_modifier+
	;
enum_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	;
enum_member_declarations 
	: enum_member_declaration ( COMMA  enum_member_declaration )*
	  -> ^(ENUM_MEMBER_DECLARATIONS enum_member_declaration+)
	;
/*
enum_member_declaration 
	: attributes? IDENTIFIER
	| attributes? IDENTIFIER ASSIGNMENT constant_expression
	;
*/
enum_member_declaration 
  : attributes? IDENTIFIER (ASSIGNMENT constant_expression)?
    -> ^(ENUM_MEMBER_DECLARATION attributes? IDENTIFIER ^(ENUM_MEMBER_INITIALIZER constant_expression)? )
  ;

//B.2.12 Delegates
/** is not used anymore */
delegate_declaration 
	: attributes? delegate_modifiers? DELEGATE return_type IDENTIFIER variant_type_parameter_list? 
	    OPEN_PARENS formal_parameter_list? CLOSE_PARENS type_parameter_constraints_clauses? SEMICOLON
	;
delegate_modifiers 
	: delegate_modifier ( delegate_modifier )*
	;
delegate_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| delegate_modifier_unsafe
	;


//B.2.13 Attributes
// not used anymore; we use global_attribute_section+ directly
global_attributes 
	: global_attribute_sections
	;
global_attribute_sections 
	: global_attribute_section+
	;
/*
global_attribute_section 
	: OPEN_BRACKET global_attribute_target_specifier attribute_list CLOSE_BRACKET
	| OPEN_BRACKET global_attribute_target_specifier attribute_list COMMA CLOSE_BRACKET
	;
*/
global_attribute_section 
  : OPEN_BRACKET! global_attribute_target_specifier attribute_list COMMA!? CLOSE_BRACKET!
  ;
global_attribute_target_specifier 
	: global_attribute_target COLON!
	;
global_attribute_target 
  : keyword
  | IDENTIFIER
  ;
/*
global_attribute_target 
	: ASSEMBLY
	| MODULE
	;
*/
attributes 
	: attribute_sections
	;
attribute_sections 
	: attribute_section+
	  -> ^(ATTRIBUTES attribute_section+)
	;
/*
attribute_section 
	: OPEN_BRACKET attribute_target_specifier? attribute_list CLOSE_BRACKET
	| OPEN_BRACKET attribute_target_specifier? attribute_list COMMA CLOSE_BRACKET
	;
*/
attribute_section 
  : OPEN_BRACKET! attribute_target_specifier? attribute_list COMMA!? CLOSE_BRACKET!
  ;
attribute_target_specifier 
	: attribute_target COLON
	  -> ^(ATTRIBUTE_TARGET attribute_target)
	;
attribute_target 
  : keyword | IDENTIFIER
  ;
/*
attribute_target 
	: FIELD
	| EVENT
	| METHOD
	| PARAM
	| PROPERTY
	| RETURN
	| TYPE
	;
*/
attribute_list 
	: attribute ( COMMA  attribute )*
	  -> ^(ATTRIBUTE_LIST  attribute+)
	;
attribute 
	: attribute_name attribute_arguments?
	  -> ^(ATTRIBUTE   attribute_name   attribute_arguments?)
	;
attribute_name 
	: type_name
	  -> ^(ATTRIBUTE_NAME  type_name)
	;
/*
attribute_arguments 
	: OPEN_PARENS positional_argument_list? CLOSE_PARENS
	| OPEN_PARENS positional_argument_list COMMA named_argument_list CLOSE_PARENS
	| OPEN_PARENS named_argument_list CLOSE_PARENS
	;
*/
/* positional_argument_list includes named_argument_list */ 
attribute_arguments 
  : OPEN_PARENS! positional_argument_list? CLOSE_PARENS!
  ;
positional_argument_list 
	: positional_argument (COMMA positional_argument )*
	  -> ^(POSITIONAL_ARGUMENT_LIST  positional_argument+)
	;
/** expression */
positional_argument 
	: attribute_argument_expression
	;
/** starts with "IDENTIFIER ASSIGNMENT expression" */
named_argument_list 
	: named_argument ( COMMA  named_argument )*
	;
/** IDENTIFIER ASSIGNMENT expression */
named_argument 
	: IDENTIFIER ASSIGNMENT attribute_argument_expression
	;
attribute_argument_expression 
	: expression
	;


//B.3 Grammar extensions for unsafe code
class_modifier_unsafe 
	: UNSAFE
	;
struct_modifier_unsafe 
	: UNSAFE
	;
interface_modifier_unsafe 
	: UNSAFE
	;
delegate_modifier_unsafe 
	: UNSAFE
	;
field_modifier_unsafe 
	: UNSAFE
	;
method_modifier_unsafe 
	: UNSAFE
	;
property_modifier_unsafe 
	: UNSAFE
	;
event_modifier_unsafe 
	: UNSAFE
	;
indexer_modifier_unsafe 
	: UNSAFE
	;
operator_modifier_unsafe 
	: UNSAFE
	;
constructor_modifier_unsafe 
	: UNSAFE
	;
/*
destructor_declaration_unsafe 
	: attributes? EXTERN? UNSAFE? TILDE IDENTIFIER OPEN_PARENS CLOSE_PARENS destructor_body
	| attributes? UNSAFE? EXTERN? TILDE IDENTIFIER OPEN_PARENS CLOSE_PARENS destructor_body
	;
*/
destructor_declaration_unsafe 
  : attributes?
    ( EXTERN? UNSAFE?
    | UNSAFE EXTERN
    )  
    TILDE^ IDENTIFIER OPEN_PARENS CLOSE_PARENS destructor_body
  ;
/*
static_constructor_modifiers_unsafe 
	: EXTERN? UNSAFE? STATIC
	| UNSAFE? EXTERN? STATIC
	| EXTERN? STATIC UNSAFE?
	| UNSAFE? STATIC EXTERN?
	| STATIC EXTERN? UNSAFE?
	| STATIC UNSAFE? EXTERN?
	;
*/
static_constructor_modifiers_unsafe 
  : (EXTERN | UNSAFE)? STATIC
  | EXTERN UNSAFE STATIC
  | UNSAFE EXTERN STATIC
  | EXTERN STATIC UNSAFE
  | UNSAFE STATIC EXTERN
  | STATIC (EXTERN | UNSAFE)
  | STATIC EXTERN UNSAFE
  | STATIC UNSAFE EXTERN
  ;
/** starts with UNSAFE or FIXED */
embedded_statement_unsafe 
	: unsafe_statement
	| fixed_statement
	;
unsafe_statement 
	: UNSAFE block
	;
type_unsafe 
	: pointer_type
	;
/*
pointer_type 
	: unmanaged_type STAR
	| VOID STAR
	;
*/
// for explanations, see http://www.antlr.org/wiki/display/ANTLR3/Left-Recursion+Removal+-+Print+Edition
pointer_type 
@init {
    boolean allowAll = true;
}
  : ( simple_type
	  | class_type
	  | VOID {allowAll = false;}
  ) ( {allowAll}? => rank_specifier
    | {allowAll}? => INTERR
    | STAR {allowAll = true;}
    )* STAR
  ;
//pointer_type
//    :    type_name (rank_specifier | INTERR)* STAR
//    |    simple_type (rank_specifier | INTERR)* STAR
//    |    enum_type (rank_specifier | INTERR)* STAR
//    |    class_type (rank_specifier | INTERR)* STAR
//    |    interface_type (rank_specifier | INTERR)* STAR
//    |    delegate_type (rank_specifier | INTERR)* STAR
//    |    type_parameter (rank_specifier | INTERR)* STAR
//    |    pointer_type (rank_specifier | INTERR)* STAR
//    |    VOID STAR
//    ;
unmanaged_type 
	: type
	;
/*
primary_no_array_creation_expression_unsafe 
	: pointer_member_access
	| pointer_element_access
	| sizeof_expression
	;
*/
primary_no_array_creation_expression_unsafe 
	: primary_expression
	;
/** starts with STAR or AMP */
unary_expression_unsafe 
	: pointer_indirection_expression
	| addressof_expression
	;
pointer_indirection_expression 
	: STAR unary_expression
	;
/* not used anymore; included in primary_expression
pointer_member_access 
	: primary_expression OP_PTR IDENTIFIER
	;
*/
/* not used anymore; included in primary_no_array_creation_expression
pointer_element_access 
	: primary_no_array_creation_expression OPEN_BRACKET expression CLOSE_BRACKET
	;
*/
/** AMP unary_expression */
addressof_expression 
	: AMP unary_expression
	;
sizeof_expression 
	: SIZEOF OPEN_PARENS unmanaged_type CLOSE_PARENS
	;
fixed_statement 
	: FIXED OPEN_PARENS pointer_type fixed_pointer_declarators CLOSE_PARENS embedded_statement
	;
fixed_pointer_declarators 
	: fixed_pointer_declarator ( COMMA  fixed_pointer_declarator )*
	;
fixed_pointer_declarator 
	: IDENTIFIER ASSIGNMENT fixed_pointer_initializer
	;
/*
fixed_pointer_initializer 
	: AMP variable_reference
	| expression
	;
*/
fixed_pointer_initializer 
  : (AMP) => AMP variable_reference
  | expression
  ;
struct_member_declaration_unsafe 
	: fixed_size_buffer_declaration
	;
fixed_size_buffer_declaration 
	: attributes? fixed_size_buffer_modifiers? FIXED buffer_element_type fixed_size_buffer_declarators SEMICOLON
	;
fixed_size_buffer_modifiers 
	: fixed_size_buffer_modifier+
	;
fixed_size_buffer_modifier 
	: NEW
	| PUBLIC
	| PROTECTED
	| INTERNAL
	| PRIVATE
	| UNSAFE
	;
buffer_element_type 
	: type
	;
fixed_size_buffer_declarators 
	: fixed_size_buffer_declarator+
	;
fixed_size_buffer_declarator 
	: IDENTIFIER OPEN_BRACKET constant_expression CLOSE_BRACKET
	;
/** starts with STACKALLOC */
local_variable_initializer_unsafe 
	: stackalloc_initializer
	;
stackalloc_initializer 
	: STACKALLOC unmanaged_type OPEN_BRACKET expression CLOSE_BRACKET
	;

	
// ---------------------------------- rules not defined in the original parser ----------
// ---------------------------------- rules not defined in the original parser ----------
// ---------------------------------- rules not defined in the original parser ----------
// ---------------------------------- rules not defined in the original parser ----------


from_contextual_keyword
  : {input.LT(1).getText().equals("from")}? IDENTIFIER
  ;
let_contextual_keyword
  : {input.LT(1).getText().equals("let")}? IDENTIFIER
  ;
where_contextual_keyword
  : {input.LT(1).getText().equals("where")}? IDENTIFIER
  ;
join_contextual_keyword
  : {input.LT(1).getText().equals("join")}? IDENTIFIER
  ;
on_contextual_keyword
  : {input.LT(1).getText().equals("on")}? IDENTIFIER
  ;
equals_contextual_keyword
  : {input.LT(1).getText().equals("equals")}? IDENTIFIER
  ;
into_contextual_keyword
  : {input.LT(1).getText().equals("into")}? IDENTIFIER
  ;
orderby_contextual_keyword
  : {input.LT(1).getText().equals("orderby")}? IDENTIFIER
  ;
ascending_contextual_keyword
  : {input.LT(1).getText().equals("ascending")}? IDENTIFIER
  ;
descending_contextual_keyword
  : {input.LT(1).getText().equals("descending")}? IDENTIFIER
  ;
select_contextual_keyword
  : {input.LT(1).getText().equals("select")}? IDENTIFIER
  ;
group_contextual_keyword
  : {input.LT(1).getText().equals("group")}? IDENTIFIER
  ;
by_contextual_keyword
  : {input.LT(1).getText().equals("by")}? IDENTIFIER
  ;
partial_contextual_keyword
  : {input.LT(1).getText().equals("partial")}? IDENTIFIER
  ;
alias_contextual_keyword
  : {input.LT(1).getText().equals("alias")}? IDENTIFIER
  ;
yield_contextual_keyword
  : {input.LT(1).getText().equals("yield")}? IDENTIFIER
  ;
get_contextual_keyword
  : {input.LT(1).getText().equals("get")}? IDENTIFIER
  ;
set_contextual_keyword
  : {input.LT(1).getText().equals("set")}? IDENTIFIER
  ;
add_contextual_keyword
  : {input.LT(1).getText().equals("add")}? IDENTIFIER
  ;
remove_contextual_keyword
  : {input.LT(1).getText().equals("remove")}? IDENTIFIER
  ;
dynamic_contextual_keyword
  : {input.LT(1).getText().equals("dynamic")}? IDENTIFIER
  ;
arglist
  : {input.LT(1).getText().equals("__arglist")}? IDENTIFIER
  ;
right_arrow
  : first=ASSIGNMENT second=GT {$first.index + 1 == $second.index}? // Nothing between the tokens?
  ;
right_shift
  : first=GT second=GT {$first.index + 1 == $second.index}? // Nothing between the tokens?
    -> OP_RIGHT_SHIFT
  ;
right_shift_assignment
  : first=GT second=OP_GE {$first.index + 1 == $second.index}? // Nothing between the tokens?
    -> OP_RIGHT_SHIFT_ASSIGNMENT
  ;
literal
  : boolean_literal
  | INTEGER_LITERAL
  | REAL_LITERAL
  | CHARACTER_LITERAL
  | STRING_LITERAL
  | NULL
  ;
boolean_literal
  : TRUE
  | FALSE
  ;
//B.1.7 Keywords
keyword
  : ABSTRACT
  | AS
  | BASE
  | BOOL
  | BREAK
  | BYTE
  | CASE
  | CATCH
  | CHAR
  | CHECKED
  | CLASS
  | CONST
  | CONTINUE
  | DECIMAL
  | DEFAULT
  | DELEGATE
  | DO
  | DOUBLE
  | ELSE
  | ENUM
  | EVENT
  | EXPLICIT
  | EXTERN
  | FALSE
  | FINALLY
  | FIXED
  | FLOAT
  | FOR
  | FOREACH
  | GOTO
  | IF
  | IMPLICIT
  | IN
  | INT
  | INTERFACE
  | INTERNAL
  | IS
  | LOCK
  | LONG
  | NAMESPACE
  | NEW
  | NULL
  | OBJECT
  | OPERATOR
  | OUT
  | OVERRIDE
  | PARAMS
  | PRIVATE
  | PROTECTED
  | PUBLIC
  | READONLY
  | REF
  | RETURN
  | SBYTE
  | SEALED
  | SHORT
  | SIZEOF
  | STACKALLOC
  | STATIC
  | STRING
  | STRUCT
  | SWITCH
  | THIS
  | THROW
  | TRUE
  | TRY
  | TYPEOF
  | UINT
  | ULONG
  | UNCHECKED
  | UNSAFE
  | USHORT
  | USING
  | VIRTUAL
  | VOID
  | VOLATILE
  | WHILE
  ;

// -------------------- extra rules for modularization --------------------------------

class_definition
  : CLASS^ IDENTIFIER type_parameter_list? class_base? type_parameter_constraints_clauses?
      class_body SEMICOLON!?
  ;
struct_definition
  : STRUCT^ IDENTIFIER type_parameter_list? struct_interfaces? type_parameter_constraints_clauses?
      struct_body SEMICOLON!?
  ;
interface_definition
  : INTERFACE^ IDENTIFIER variant_type_parameter_list? interface_base?
      type_parameter_constraints_clauses? interface_body SEMICOLON!?
  ;
enum_definition
  : ENUM^ IDENTIFIER enum_base? enum_body SEMICOLON!?
  ;
delegate_definition
  : DELEGATE^ return_type IDENTIFIER variant_type_parameter_list? OPEN_PARENS
      formal_parameter_list? CLOSE_PARENS type_parameter_constraints_clauses? SEMICOLON!
  ;
event_declaration2
  : EVENT type {$common_member_declaration::type = $type.tree;}
      ( variable_declarators SEMICOLON
        -> ^(EVENT_VARS_DECL variable_declarators)
      | member_name OPEN_BRACE event_accessor_declarations CLOSE_BRACE
        -> ^(EVENT_PROP_DECL type member_name event_accessor_declarations)
      )
  ;
field_declaration2
  : variable_declarators SEMICOLON
    -> ^(FIELD_DECL variable_declarators)
  ;
property_declaration2
  : member_name OPEN_BRACE accessor_declarations CLOSE_BRACE
    -> ^(PROPERTY_DECL  member_name  accessor_declarations)
  ;
constant_declaration2
  : CONST! t=type! constant_declarators[t.tree] SEMICOLON!
  ;
indexer_declaration2
  : THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET
      OPEN_BRACE accessor_declarations CLOSE_BRACE
    -> ^(INDEXER_DECL formal_parameter_list  accessor_declarations)
  ;
destructor_definition
  : TILDE^ IDENTIFIER OPEN_PARENS! CLOSE_PARENS! destructor_body
  ;
constructor_declaration2
  : IDENTIFIER OPEN_PARENS formal_parameter_list? CLOSE_PARENS constructor_initializer? body
    -> ^(CONSTRUCTOR_DECL  IDENTIFIER  formal_parameter_list? constructor_initializer?  body)
  ;
method_declaration2
  : method_member_name type_parameter_list? OPEN_PARENS formal_parameter_list? CLOSE_PARENS
      type_parameter_constraints_clauses? method_body
    -> ^(METHOD_DECL  method_member_name  type_parameter_list?  formal_parameter_list?  
            type_parameter_constraints_clauses?   method_body? )
  ;
// added by chw to allow detection of type parameters for methods
method_member_name
  : method_member_name2 -> ^(MEMBER_NAME ^(NAMESPACE_OR_TYPE_NAME method_member_name2))
  ;
method_member_name2
  : ( IDENTIFIER
    | IDENTIFIER DOUBLE_COLON IDENTIFIER
    ) (type_argument_list_opt DOT IDENTIFIER)*
  ;
operator_declaration2
  : OPERATOR overloadable_operator OPEN_PARENS t1=type id1=IDENTIFIER
         (COMMA t2=type id2=IDENTIFIER)? CLOSE_PARENS operator_body
    -> ^(OPERATOR overloadable_operator ^(FIRST_OP $t1 $id1) ^(SECOND_OP $t2? $id2?) operator_body)
  ;

interface_method_declaration2
  : IDENTIFIER type_parameter_list? OPEN_PARENS formal_parameter_list? CLOSE_PARENS type_parameter_constraints_clauses? SEMICOLON
    -> ^(METHOD_DECL ^(MEMBER_NAME ^(NAMESPACE_OR_TYPE_NAME IDENTIFIER))  type_parameter_list?  formal_parameter_list?   type_parameter_constraints_clauses?)
  ;
interface_property_declaration2
  : IDENTIFIER OPEN_BRACE interface_accessors CLOSE_BRACE
    -> ^(PROPERTY_DECL ^(MEMBER_NAME ^(NAMESPACE_OR_TYPE_NAME IDENTIFIER))  interface_accessors)
  ;
interface_event_declaration2
  : EVENT type IDENTIFIER SEMICOLON -> ^(EVENT_INTERFACE_DECL type IDENTIFIER)
  ;
interface_indexer_declaration2
  : THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET OPEN_BRACE interface_accessors CLOSE_BRACE
    -> ^(INDEXER_DECL formal_parameter_list interface_accessors)
  ;
/** starts with DOT IDENTIFIER */
member_access2
  : DOT! IDENTIFIER type_argument_list_opt
  ;
method_invocation2
  : OPEN_PARENS! argument_list? CLOSE_PARENS!
  ;
object_creation_expression2
  : OPEN_PARENS argument_list? CLOSE_PARENS object_or_collection_initializer?
    -> ^(OBJECT_CREATION_EXPRESSION  argument_list? object_or_collection_initializer?)
  ;
