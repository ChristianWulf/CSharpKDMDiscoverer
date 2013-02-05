/***************************************************************************
 * Copyright 2012 by
 * + Christian-Albrechts-University of Kiel
 * + Department of Computer Science
 * + Software Engineering Group
 * and others.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ***************************************************************************/

package mapping;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertSame;
import static util.test.Assert.assertInstanceof;

import java.io.IOException;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;

import mapping.KDMElementFactory.GlobalKind;
import mapping.code.MoDiscoKDM;
import mapping.code.extern.ExternalDatatypeInfoRepository;
import mapping.code.extern.loader.DatatypeInfoCacheLoader;
import mapping.code.extern.loader.DatatypeInfoFileSystemLoader;
import mapping.code.extern.loader.IDatatypeInfoLoader;
import mapping.code.language.CSharpLanguageUnitCache;
import mapping.code.language.GenericLanguageUnitCache;
import mapping.code.namespace.NamespaceSearcher;
import mapping.code.transformator.Phase1Transformator;
import mapping.code.transformator.Phase2TransformatorNew;
import mapping.code.transformator.Phase3Transformator;

import org.antlr.runtime.RecognitionException;
import org.antlr.runtime.tree.CommonTree;
import org.eclipse.emf.common.util.EList;
import org.eclipse.gmt.modisco.omg.kdm.action.AbstractActionRelationship;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionElement;
import org.eclipse.gmt.modisco.omg.kdm.action.BlockUnit;
import org.eclipse.gmt.modisco.omg.kdm.action.Calls;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeRelationship;
import org.eclipse.gmt.modisco.omg.kdm.code.ArrayType;
import org.eclipse.gmt.modisco.omg.kdm.code.BooleanType;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.ExportKind;
import org.eclipse.gmt.modisco.omg.kdm.code.Extends;
import org.eclipse.gmt.modisco.omg.kdm.code.FloatType;
import org.eclipse.gmt.modisco.omg.kdm.code.Imports;
import org.eclipse.gmt.modisco.omg.kdm.code.IntegerType;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MemberUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodKind;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.code.ParameterKind;
import org.eclipse.gmt.modisco.omg.kdm.code.ParameterUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Signature;
import org.eclipse.gmt.modisco.omg.kdm.code.StorableUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.StringType;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Value;
import org.eclipse.gmt.modisco.omg.kdm.code.VoidType;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFactory;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;
import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;

import util.Parsing;
import util.test.Assert;

public class TestFileMapperTest {

	private static final String				RESOURCE_DECOMPILED_DIR	= "resource/test/decompiled";
	private static final String				RESOURCE_TESTCASES_DIR	= "resource/test/testcases/";
	private static GenericLanguageUnitCache	genericLanguageUnitCache;
	private static Namespace				internalGlobalNamespace;
	private static Namespace				externalGlobalNamespace;

	private IKDMMapper<CompilationUnit>		transformator;
	private CodeModel						internalCodeModel;
	private CodeModel						externalCodeModel;
	private Phase1Transformator				preTransformator;
	private Phase2TransformatorNew			memberDeclarationsTransformator;

	@BeforeClass
	public static void beforeClass() throws Exception {
		genericLanguageUnitCache = new CSharpLanguageUnitCache();
	}

	@Before
	public void before() throws Exception {
		internalCodeModel = KDMElementFactory.createGenericCodeModel("Internal CodeModel", GlobalKind.INTERNAL);
		externalCodeModel = KDMElementFactory.createGenericCodeModel("External CodeModel", GlobalKind.EXTERNAL);
		Module valueRepository = KDMElementFactory.createValueRepository(internalCodeModel);
		internalGlobalNamespace = NamespaceSearcher.getGlobalNamespaceFrom(internalCodeModel);
		externalGlobalNamespace = NamespaceSearcher.getGlobalNamespaceFrom(externalCodeModel);

		IDatatypeInfoLoader loader = new DatatypeInfoCacheLoader(new DatatypeInfoFileSystemLoader());
		ExternalDatatypeInfoRepository classInfoRepository = new ExternalDatatypeInfoRepository(
				RESOURCE_DECOMPILED_DIR, loader);
		// fileMapper = new FileMapper(languageUnitCache, internalCodeModel,
		// externalCodeModel,
		// classInfoRepository);
		preTransformator = new Phase1Transformator(internalCodeModel);
		memberDeclarationsTransformator = new Phase2TransformatorNew(genericLanguageUnitCache, internalCodeModel,
				valueRepository, externalCodeModel, classInfoRepository);
		transformator = new Phase3Transformator(genericLanguageUnitCache, internalCodeModel, valueRepository,
				externalCodeModel, classInfoRepository);
	}

	// Module.getCodeElement() = ordered
	// ClassUnit.getCodeElement = ordered
	// InterfaceUnit.getCodeElement = ordered

	@Test
	public void testTransform3() throws IOException, RecognitionException {
		final String dirName = RESOURCE_TESTCASES_DIR + "3/";
		String filename = "HelloWorld.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);

		assertEquals(0, compilationUnit.getCodeElement().size());

		// 1 internal namespace
		EList<CodeItem> groupedCodeItems = internalGlobalNamespace.getGroupedCode();
		assertEquals(groupedCodeItems.toString(), 1, groupedCodeItems.size());

		Namespace namespace = assertInstanceof(Namespace.class, groupedCodeItems.get(0));
		assertEquals("HelloWorld", namespace.getName());

		// no external namespaces
		groupedCodeItems = externalGlobalNamespace.getGroupedCode();
		assertEquals(0, groupedCodeItems.size());
	}

	@Test
	public void testTransform4() throws IOException, RecognitionException {
		final String dirName = RESOURCE_TESTCASES_DIR + "4/";
		String filename = "HelloWorld.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);

		EList<AbstractCodeElement> codeElements = compilationUnit.getCodeElement();

		// ClassUnit
		ClassUnit classUnit = assertInstanceof(ClassUnit.class, codeElements.get(0));
		assertEquals(Boolean.FALSE, classUnit.getIsAbstract());
		assertEquals("Hello", classUnit.getName());
		assertEquals(Collections.EMPTY_LIST, classUnit.getCodeElement());

		// InterfaceUnit
		InterfaceUnit interfaceUnit = assertInstanceof(InterfaceUnit.class, codeElements.get(1));
		assertEquals("IHello", interfaceUnit.getName());
		assertEquals(Collections.EMPTY_LIST, interfaceUnit.getCodeElement());

		// 1 internal namespace
		EList<CodeItem> groupedCodeItems = internalGlobalNamespace.getGroupedCode();
		assertEquals(1, groupedCodeItems.size());

		Namespace namespace = assertInstanceof(Namespace.class, groupedCodeItems.get(0));
		assertEquals("HelloWorld", namespace.getName());

		EList<CodeItem> groupedCodeItems2 = namespace.getGroupedCode();
		assertEquals(2, groupedCodeItems2.size());
		assertSame(classUnit, groupedCodeItems2.get(0));
		assertSame(interfaceUnit, groupedCodeItems2.get(1));

		// no external namespaces
		groupedCodeItems = externalGlobalNamespace.getGroupedCode();
		assertEquals(0, groupedCodeItems.size());
	}

	@Test
	public void testTransform5() throws IOException, RecognitionException {
		final String dirName = RESOURCE_TESTCASES_DIR + "5/";
		String filename = "HelloWorld.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);

		EList<AbstractCodeElement> codeElements = compilationUnit.getCodeElement();

		// ClassUnit
		AbstractCodeElement codeElement = codeElements.get(0);
		ClassUnit classUnit = assertInstanceof(ClassUnit.class, codeElement);
		assertEquals(Boolean.FALSE, classUnit.getIsAbstract());
		assertEquals("Hello", classUnit.getName());
		EList<CodeItem> codeItems = classUnit.getCodeElement();
		assertEquals(1, codeItems.size());
		// MethodUnit
		CodeItem codeItem = codeItems.get(0);
		MethodUnit methodUnit = assertInstanceof(MethodUnit.class, codeItem);
		assertEquals(ExportKind.PRIVATE, methodUnit.getExport());
		assertEquals(MethodKind.METHOD, methodUnit.getKind());
		assertEquals("Main", methodUnit.getName());
		EList<AbstractCodeElement> methodElements = methodUnit.getCodeElement();
		assertEquals(2, methodElements.size());
		Signature signature = assertInstanceof(Signature.class, methodElements.get(0));
		assertEquals("Main", signature.getName());
		EList<ParameterUnit> parameterUnits = signature.getParameterUnit();
		assertEquals(1, parameterUnits.size());
		ParameterUnit parameterUnit = parameterUnits.get(0);
		assertEquals(ParameterKind.RETURN, parameterUnit.getKind());
		assertNull(parameterUnit.getPos());

		// 1 internal namespace
		EList<CodeItem> groupedCodeItems = internalGlobalNamespace.getGroupedCode();
		assertEquals(1, groupedCodeItems.size());

		Namespace helloWorldNamespace = assertInstanceof(Namespace.class, groupedCodeItems.get(0));
		assertEquals("HelloWorld", helloWorldNamespace.getName());

		EList<CodeItem> groupedCodeItems2 = helloWorldNamespace.getGroupedCode();
		assertEquals(groupedCodeItems2.toString(), 1, groupedCodeItems2.size());
		assertSame(classUnit, groupedCodeItems2.get(0));

		// 1 external namespaces due to using directive
		groupedCodeItems = externalGlobalNamespace.getGroupedCode();
		assertEquals(1, groupedCodeItems.size());

		Namespace systemNamespace = assertInstanceof(Namespace.class, groupedCodeItems.get(0));
		assertEquals("System", systemNamespace.getName());
		assertEquals(0, systemNamespace.getGroupedCode().size());

		// using directive
		EList<AbstractCodeRelationship> relations = compilationUnit.getCodeRelation();
		assertEquals(1, relations.size());

		Imports imports = assertInstanceof(Imports.class, relations.get(0));
		assertEquals(internalGlobalNamespace, imports.getFrom());
		assertEquals(systemNamespace, imports.getTo());
	}

	@Test
	public void testTransform6() throws IOException, RecognitionException {
		final String dirName = RESOURCE_TESTCASES_DIR + "6/";
		String filename = "HelloWorld.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);

		EList<AbstractCodeElement> compilationElements = compilationUnit.getCodeElement();
		assertEquals(1, compilationElements.size());

		// ClassUnit
		AbstractCodeElement codeElement = compilationElements.get(0);
		ClassUnit classUnit = assertInstanceof(ClassUnit.class, codeElement);
		assertEquals(Boolean.FALSE, classUnit.getIsAbstract());
		assertEquals("Hello", classUnit.getName());
		EList<CodeItem> codeItems = classUnit.getCodeElement();
		assertEquals(codeItems.toString(), 1, codeItems.size());

		// MethodUnit
		MethodUnit mainMethodUnit = assertInstanceof(MethodUnit.class, codeItems.get(0));
		assertEquals(ExportKind.PRIVATE, mainMethodUnit.getExport());
		assertEquals(MethodKind.METHOD, mainMethodUnit.getKind());
		assertEquals("Main", mainMethodUnit.getName());
		EList<AbstractCodeElement> mainMethodElements = mainMethodUnit.getCodeElement();
		assertEquals(2, mainMethodElements.size());

		Signature signature = assertInstanceof(Signature.class, mainMethodElements.get(0));
		assertEquals("Main", signature.getName());
		EList<ParameterUnit> parameterUnits = signature.getParameterUnit();
		assertEquals(2, parameterUnits.size());
		ParameterUnit parameterUnit;

		parameterUnit = parameterUnits.get(0);
		assertEquals(ParameterKind.RETURN, parameterUnit.getKind());
		assertNull(parameterUnit.getPos());
		assertInstanceof(VoidType.class, parameterUnit.getType());

		parameterUnit = parameterUnits.get(1);
		assertEquals("args", parameterUnit.getName());
		assertEquals(0, parameterUnit.getPos().intValue());
		// should be BY_REFERENCE, but is not yet supported
		assertEquals(ParameterKind.UNKNOWN, parameterUnit.getKind());
		ArrayType arrayType = assertInstanceof(ArrayType.class, parameterUnit.getType());
		assertInstanceof(StringType.class, arrayType.getItemUnit().getType());

		// 1 internal namespace
		EList<CodeItem> groupedCodeItems = internalGlobalNamespace.getGroupedCode();
		assertEquals(groupedCodeItems.toString(), 1, groupedCodeItems.size());

		Namespace namespace = assertInstanceof(Namespace.class, groupedCodeItems.get(0));
		assertEquals("HelloWorld", namespace.getName());

		EList<CodeItem> groupedCodeItems2 = namespace.getGroupedCode();
		assertEquals(groupedCodeItems2.toString(), 1, groupedCodeItems2.size());
		assertSame(classUnit, groupedCodeItems2.get(0));

		// 1 external namespace: System
		groupedCodeItems = externalGlobalNamespace.getGroupedCode();
		assertEquals(1, groupedCodeItems.size());

		Namespace systemNamespace = assertInstanceof(Namespace.class, groupedCodeItems.get(0));
		assertEquals("System", systemNamespace.getName());
		groupedCodeItems = systemNamespace.getGroupedCode();
		assertEquals(1, groupedCodeItems.size());

		ClassUnit consoleClassUnit = assertInstanceof(ClassUnit.class, groupedCodeItems.get(0));
		assertEquals("Console", consoleClassUnit.getName());
		assertEquals(Boolean.FALSE, consoleClassUnit.getIsAbstract());
		EList<CodeItem> consoleElements = consoleClassUnit.getCodeElement();
		assertEquals(1, consoleElements.size());

		MethodUnit writeLineMethodUnit = assertInstanceof(MethodUnit.class, consoleElements.get(0));
		assertEquals("WriteLine", writeLineMethodUnit.getName());

		// Method call
		// is not generated by MoDisco
		// EList<EntryFlow> entryFlows = methodUnit.getEntryFlow();
		// assertEquals(1, entryFlows.size());

		BlockUnit mainMethodBlock = assertInstanceof(BlockUnit.class, mainMethodElements.get(1));
		EList<AbstractCodeElement> mainMethodBlockElements = mainMethodBlock.getCodeElement();
		assertEquals(1, mainMethodBlockElements.size());

		ActionElement writeLineExpr = assertInstanceof(ActionElement.class, mainMethodBlockElements.get(0));
		assertEquals(MoDiscoKDM.EXPRESSION_STATEMENT, writeLineExpr.getName());
		assertEquals(MoDiscoKDM.EXPRESSION_STATEMENT, writeLineExpr.getKind());
		EList<AbstractCodeElement> exprElements = writeLineExpr.getCodeElement();
		assertEquals(1, exprElements.size());

		ActionElement writeLineInvocation = assertInstanceof(ActionElement.class, exprElements.get(0));
		assertEquals(MoDiscoKDM.METHOD_INVOCATION, writeLineInvocation.getName());
		assertEquals(MoDiscoKDM.METHOD_INVOCATION, writeLineInvocation.getKind());
		EList<AbstractCodeElement> invocationElements = writeLineInvocation.getCodeElement();
		assertEquals(1, invocationElements.size());

		Value value = assertInstanceof(Value.class, invocationElements.get(0));
		assertEquals("Hello World!", value.getName());
		assertInstanceof(StringType.class, value.getType());
		assertEquals("Hello World!".length(), value.getSize().intValue());

		EList<AbstractActionRelationship> writeLineInvocationRelations = writeLineInvocation.getActionRelation();
		assertEquals(1, writeLineInvocationRelations.size());

		Calls calls = assertInstanceof(Calls.class, writeLineInvocationRelations.get(0));
		assertEquals(writeLineInvocation, calls.getFrom());
		assertEquals(writeLineMethodUnit, calls.getTo());

		// is not generated by MoDisco
		// Addresses addresses = assertInstanceof(Addresses.class,
		// actionRelations.get(0));
		// assertEquals(writeLineInvocation, addresses.getFrom());
		// assertEquals(consoleClassUnit, addresses.getTo());
		// is not generated by MoDisco
		// Reads reads = assertInstanceof(Reads.class, actionRelations.get(1));
		// assertEquals(writeLineInvocation, reads.getFrom());
		// assertEquals(value, reads.getTo());

		// TODO see MoDisco
	}

	@Test
	public void testTransform8() throws IOException, RecognitionException {
		final String dirName = RESOURCE_TESTCASES_DIR + "8/";
		String filename = "HelloWorld.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);

		EList<AbstractCodeElement> codeElements = compilationUnit.getCodeElement();

		// external code model
		EList<AbstractCodeElement> externalCodeElements = externalCodeModel.getCodeElement();
		assertEquals(2, externalCodeElements.size());

		ClassUnit consoleClass = assertInstanceof(ClassUnit.class, externalCodeElements.get(1));
		assertEquals("Console", consoleClass.getName());
		assertEquals(Boolean.FALSE, consoleClass.getIsAbstract());
		assertEquals(1, consoleClass.getCodeElement().size());

		// ClassUnit
		AbstractCodeElement codeElement = codeElements.get(0);
		ClassUnit classUnit = assertInstanceof(ClassUnit.class, codeElement);
		assertEquals(Boolean.FALSE, classUnit.getIsAbstract());
		assertEquals("Hello", classUnit.getName());
		EList<CodeItem> codeItems = classUnit.getCodeElement();
		assertEquals(2, codeItems.size());
		// field
		CodeItem consoleField = codeItems.get(0);
		// TODO
		// MethodUnit
		CodeItem codeItem = codeItems.get(1);
		MethodUnit methodUnit = assertInstanceof(MethodUnit.class, codeItem);
		assertEquals(ExportKind.PRIVATE, methodUnit.getExport());
		assertEquals(MethodKind.METHOD, methodUnit.getKind());
		assertEquals("Main", methodUnit.getName());
		EList<AbstractCodeElement> methodElements = methodUnit.getCodeElement();
		assertEquals(2, methodElements.size());
		Signature signature = assertInstanceof(Signature.class, methodElements.get(0));
		assertEquals("Main", signature.getName());
		EList<ParameterUnit> parameterUnits = signature.getParameterUnit();
		assertEquals(2, parameterUnits.size());

		ParameterUnit parameterUnit = parameterUnits.get(0);
		assertEquals(ParameterKind.RETURN, parameterUnit.getKind());
		assertNull(parameterUnit.getPos());

		ParameterUnit argsParameter = parameterUnits.get(1);
		assertEquals(ParameterKind.UNKNOWN, argsParameter.getKind());
		assertEquals(Integer.valueOf(0), argsParameter.getPos());
		ArrayType stringArrayType = assertInstanceof(ArrayType.class, argsParameter.getType());
		// TODO

		ActionElement mainMethodBlock = assertInstanceof(ActionElement.class, methodElements.get(1));
		// TODO

		// 1 internal namespace
		EList<CodeItem> groupedCodeItems = internalGlobalNamespace.getGroupedCode();
		assertEquals(1, groupedCodeItems.size());

		Namespace helloWorldNamespace = assertInstanceof(Namespace.class, groupedCodeItems.get(0));
		assertEquals("HelloWorld", helloWorldNamespace.getName());

		EList<CodeItem> groupedCodeItems2 = helloWorldNamespace.getGroupedCode();
		assertEquals(groupedCodeItems2.toString(), 1, groupedCodeItems2.size());
		assertSame(classUnit, groupedCodeItems2.get(0));

		// 1 external namespaces due to using directive
		groupedCodeItems = externalGlobalNamespace.getGroupedCode();
		assertEquals(1, groupedCodeItems.size());

		Namespace systemNamespace = assertInstanceof(Namespace.class, groupedCodeItems.get(0));
		assertEquals("System", systemNamespace.getName());
		assertEquals(1, systemNamespace.getGroupedCode().size());

		assertEquals(consoleClass, systemNamespace.getGroupedCode().get(0));

		// using directive
		EList<AbstractCodeRelationship> relations = compilationUnit.getCodeRelation();
		assertEquals(1, relations.size());

		Imports imports = assertInstanceof(Imports.class, relations.get(0));
		assertEquals(internalGlobalNamespace, imports.getFrom());
		assertEquals(systemNamespace, imports.getTo());
	}

	@Test
	public void testTransform9() throws IOException, RecognitionException {
		final String dirName = RESOURCE_TESTCASES_DIR + "9/";
		String filename = "HelloWorld.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);

		EList<AbstractCodeElement> codeElements = compilationUnit.getCodeElement();
		EList<AbstractCodeRelationship> codeRelations = compilationUnit.getCodeRelation();

		assertEquals(1, codeElements.size());
		// TODO

		assertEquals(2, codeRelations.size());

		Imports importSystem = assertInstanceof(Imports.class, codeRelations.get(0));
		importSystem.getFrom(); // TODO
		Namespace usingSystem = assertInstanceof(Namespace.class, importSystem.getTo());
		assertEquals("System", usingSystem.getName());
		assertEquals("System", KDMElementFactory.getQualifiedNameAttribute(usingSystem).getValue());

		Imports importSystemBuffer = assertInstanceof(Imports.class, codeRelations.get(1));
		importSystemBuffer.getFrom(); // TODO
		Namespace usingSystemBuffer = assertInstanceof(Namespace.class, importSystemBuffer.getTo());
		assertEquals("Buffer", usingSystemBuffer.getName());
		assertEquals("System.Buffer", KDMElementFactory.getQualifiedNameAttribute(usingSystemBuffer).getValue());
	}

	@SuppressWarnings("unchecked")
	@Test
	public void testTransform10() throws IOException, RecognitionException {
		final String dirName = RESOURCE_TESTCASES_DIR + "10/";
		String filename = "HelloWorld.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);
		EList<AbstractCodeElement> codeElements = compilationUnit.getCodeElement();

		// ClassUnit
		AbstractCodeElement codeElement = codeElements.get(0);
		ClassUnit classUnit = assertInstanceof(ClassUnit.class, codeElement);
		assertEquals(Boolean.FALSE, classUnit.getIsAbstract());
		assertEquals("Hello", classUnit.getName());
		EList<CodeItem> codeItems = classUnit.getCodeElement();
		assertEquals(codeItems.toString(), 13, codeItems.size());

		// 1 internal namespace
		EList<CodeItem> groupedCodeItems = internalGlobalNamespace.getGroupedCode();
		assertEquals(1, groupedCodeItems.size());

		Namespace namespace = assertInstanceof(Namespace.class, groupedCodeItems.get(0));
		assertEquals("HelloWorld", namespace.getName());

		EList<CodeItem> groupedCodeItems2 = namespace.getGroupedCode();
		assertEquals(groupedCodeItems2.toString(), 1, groupedCodeItems2.size());
		assertSame(classUnit, groupedCodeItems2.get(0));

		// no external namespaces
		groupedCodeItems = externalGlobalNamespace.getGroupedCode();
		assertEquals(groupedCodeItems.toString(), 1, groupedCodeItems.size());

		Namespace systemNamespace = assertInstanceof(Namespace.class, groupedCodeItems.get(0));
		assertEquals("System", systemNamespace.getName());
		assertEquals(1, systemNamespace.getGroupedCode().size());

		ClassUnit uriClass = assertInstanceof(ClassUnit.class, systemNamespace.getGroupedCode().get(0));
		assertEquals("Uri", uriClass.getName());

		// check variable declarations
		MemberUnit memberUnit;

		// public string var1;
		memberUnit = assertInstanceof(MemberUnit.class, codeItems.get(0));
		Assert.assertFieldDeclaration(Arrays.asList("public"), datatypesAsList(StringType.class), "var1", memberUnit);

		// protected String var2a, var2b; // from System.String
		memberUnit = assertInstanceof(MemberUnit.class, codeItems.get(1));
		Assert.assertFieldDeclaration(Arrays.asList("protected"), datatypesAsList(StringType.class), "var2a",
				memberUnit);
		memberUnit = assertInstanceof(MemberUnit.class, codeItems.get(2));
		Assert.assertFieldDeclaration(Arrays.asList("protected"), datatypesAsList(StringType.class), "var2b",
				memberUnit);

		// protected internal int var3;
		memberUnit = assertInstanceof(MemberUnit.class, codeItems.get(3));
		Assert.assertFieldDeclaration(Arrays.asList("protected", "internal"), datatypesAsList(IntegerType.class),
				"var3", memberUnit);

		// internal float var4;
		memberUnit = assertInstanceof(MemberUnit.class, codeItems.get(4));
		Assert.assertFieldDeclaration(Arrays.asList("internal"), datatypesAsList(FloatType.class), "var4", memberUnit);

		// private bool var5;
		memberUnit = assertInstanceof(MemberUnit.class, codeItems.get(5));
		Assert.assertFieldDeclaration(Arrays.asList("private"), datatypesAsList(BooleanType.class), "var5", memberUnit);

		// readonly short var6;
		memberUnit = assertInstanceof(MemberUnit.class, codeItems.get(6));
		Assert.assertFieldDeclaration(Arrays.asList("readonly"), datatypesAsList(IntegerType.class), "var6", memberUnit);

		// const int* var7;
		StorableUnit constDecl;
		constDecl = assertInstanceof(StorableUnit.class, codeItems.get(7));
		assertEquals("var7", constDecl.getName());
		// assertSame(, constDecl.getType());
		// Assert.assertFieldDeclaration(Arrays.asList("const"),
		// datatypesAsList(PointerType.class, IntegerType.class), "var7",
		// constDecl);

		// public const bool** var8;
		constDecl = assertInstanceof(StorableUnit.class, codeItems.get(8));
		assertEquals("var8", constDecl.getName());
		// Assert.assertFieldDeclaration(
		// Arrays.asList("public", "const"),
		// datatypesAsList(PointerType.class, PointerType.class,
		// BooleanType.class), "var8", memberUnit);

		// internal protected string[] var9;
		memberUnit = assertInstanceof(MemberUnit.class, codeItems.get(9));
		Assert.assertFieldDeclaration(Arrays.asList("internal", "protected"),
				datatypesAsList(ArrayType.class, StringType.class), "var9", memberUnit);

		// public static readonly Uri UriSchemeFile; // from System.Uri
		constDecl = assertInstanceof(StorableUnit.class, codeItems.get(10));
		assertEquals("UriSchemeFile1", constDecl.getName());
		assertSame(uriClass, constDecl.getType());
		// Assert.assertFieldDeclaration(
		// Arrays.asList("public", "static", "readonly"),
		// datatypesAsList(uriClass), "UriSchemeFile1", storableUnit);

		// public static String prop1
		StorableUnit propDecl;
		propDecl = assertInstanceof(StorableUnit.class, codeItems.get(12));
		assertEquals("prop1", propDecl.getName());
		assertSame("String", propDecl.getType().getName());

		// TODO check the other member variables
	}

	@Test
	public void testTransform11() throws IOException, RecognitionException {
		final String dirName = RESOURCE_TESTCASES_DIR + "11/";
		String filename = "HelloWorld.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		tranform(ast);
		CompilationUnit compilationUnit = transformator.getMappingResult().get(0);

	}

	@Test
	public void testTransform12() throws IOException, RecognitionException {
		final String dirName = RESOURCE_TESTCASES_DIR + "12/";
		String filename = "HelloWorld.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		tranform(ast);

		// check namespace module
		Module namespaceModule = assertInstanceof(Module.class, internalGlobalNamespace.eContainer());
		assertEquals(KDMElementFactory.NAMESPACES_MODULE, namespaceModule.getName());
		// 2 internal namespaces
		EList<AbstractCodeElement> namespaces = namespaceModule.getCodeElement();
		assertEquals(5, namespaces.size());
		Namespace globalNamespace = assertInstanceof(Namespace.class, namespaces.get(0));
		assertEquals(KDMElementFactory.GLOBAL_NAMESPACE_NAME, globalNamespace.getName());
		Namespace firstNamespace = assertInstanceof(Namespace.class, namespaces.get(1));
		assertEquals("first", firstNamespace.getName());
		Namespace secondNamespace = assertInstanceof(Namespace.class, namespaces.get(2));
		assertEquals("second", secondNamespace.getName());
		Namespace thirdNamespace = assertInstanceof(Namespace.class, namespaces.get(3));
		assertEquals("third", thirdNamespace.getName());
		Namespace fourthNamespace = assertInstanceof(Namespace.class, namespaces.get(4));
		assertEquals("fourth", fourthNamespace.getName());
		// all namespaces belong to the namespace module
		assertSame(namespaceModule, globalNamespace.eContainer());
		assertSame(namespaceModule, firstNamespace.eContainer());
		assertSame(namespaceModule, secondNamespace.eContainer());
		assertSame(namespaceModule, thirdNamespace.eContainer());
		assertSame(namespaceModule, fourthNamespace.eContainer());
		// check namespace hierarchy
		assertSame(firstNamespace, globalNamespace.getGroupedCode().get(0));
		assertSame(secondNamespace, firstNamespace.getGroupedCode().get(0));
		assertEquals(0, secondNamespace.getGroupedCode().size());
		assertSame(thirdNamespace, globalNamespace.getGroupedCode().get(1));
		assertSame(fourthNamespace, thirdNamespace.getGroupedCode().get(0));
		assertEquals(1, fourthNamespace.getGroupedCode().size());
	}

	@Test
	// check using directive behavior
	public void testTransform13() throws IOException, RecognitionException {
		final String dirName1 = RESOURCE_TESTCASES_DIR + "13/";
		String filename1 = "MyNamespace.Test.cs";
		final String dirName2 = RESOURCE_TESTCASES_DIR + "13/";
		String filename2 = "ZHelloWorld.cs";

		// 1. transformation
		CommonTree ast1 = Parsing.loadASTFromFile(dirName1 + filename1);
		SourceFile sf1 = SourceFactory.eINSTANCE.createSourceFile();
		preTransformator.transform(ast1, sf1);
		internalCodeModel.getCodeElement().addAll(preTransformator.getMappingResult());

		CommonTree ast2 = Parsing.loadASTFromFile(dirName2 + filename2);
		SourceFile sf2 = SourceFactory.eINSTANCE.createSourceFile();
		preTransformator.transform(ast2, sf2);
		internalCodeModel.getCodeElement().addAll(preTransformator.getMappingResult());

		// 2. transformation
		memberDeclarationsTransformator.transform(ast1, sf1);
		CompilationUnit compilationUnit1 = memberDeclarationsTransformator.getMappingResult().get(0);

		memberDeclarationsTransformator.transform(ast2, sf2);
		CompilationUnit compilationUnit2 = memberDeclarationsTransformator.getMappingResult().get(0);

		// MyNamespace.Test.cs
		EList<AbstractCodeElement> codeElements1 = compilationUnit1.getCodeElement();
		assertEquals(1, codeElements1.size());

		ClassUnit testClass = assertInstanceof(ClassUnit.class, codeElements1.get(0));
		assertEquals("Test", testClass.getName());

		// ZHelloWorld.cs
		EList<AbstractCodeElement> codeElements2 = compilationUnit2.getCodeElement();
		assertEquals(codeElements2.toString(), 1, codeElements2.size());

		ClassUnit zhelloWorldClass = assertInstanceof(ClassUnit.class, codeElements2.get(0));
		assertEquals("ZHelloWorld", zhelloWorldClass.getName());

		// 3. transformation
		transformator.transform(ast1, sf1);
		CompilationUnit compilationUnit1b = transformator.getMappingResult().get(0);
		assertSame(compilationUnit1, compilationUnit1b);

		transformator.transform(ast2, sf2);
		CompilationUnit compilationUnit2b = transformator.getMappingResult().get(0);
		assertSame(compilationUnit2, compilationUnit2b);

		// MyNamespace.Test.cs
		assertEquals(Boolean.FALSE, testClass.getIsAbstract());

		// check namespace module
		Module namespaceModule = assertInstanceof(Module.class, internalGlobalNamespace.eContainer());
		assertEquals(KDMElementFactory.NAMESPACES_MODULE, namespaceModule.getName());
		// internal namespace
		EList<AbstractCodeElement> namespaces = namespaceModule.getCodeElement();
		assertEquals(namespaces.toString(), 2, namespaces.size());
		Namespace globalNamespace = assertInstanceof(Namespace.class, namespaces.get(0));
		assertEquals(KDMElementFactory.GLOBAL_NAMESPACE_NAME, globalNamespace.getName());
		Namespace myNamespace = assertInstanceof(Namespace.class, namespaces.get(1));
		assertEquals("MyNamespace", myNamespace.getName());
		assertSame(testClass, myNamespace.getGroupedCode().get(0));
		// check namespace hierarchy
		assertSame(myNamespace, globalNamespace.getGroupedCode().get(0));

		// ZHelloWorld.cs
		assertEquals(Boolean.FALSE, zhelloWorldClass.getIsAbstract());
		EList<CodeItem> hwCodeElements = zhelloWorldClass.getCodeElement();
		assertEquals(2, hwCodeElements.size());

		MemberUnit testField = assertInstanceof(MemberUnit.class, hwCodeElements.get(0));
		assertEquals("t", testField.getName());
		assertEquals(testClass, testField.getType());

		MethodUnit mainMethod = assertInstanceof(MethodUnit.class, hwCodeElements.get(1));
		assertEquals("Main", mainMethod.getName());
	}

	@Test
	public void testTransform21() throws IOException, RecognitionException {
		final String dirName = RESOURCE_TESTCASES_DIR + "21/";
		String filename = "NestedClass.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);

		EList<AbstractCodeElement> codeElements = compilationUnit.getCodeElement();
		EList<AbstractCodeRelationship> codeRelations = compilationUnit.getCodeRelation();
		// CompilationUnit
		assertEquals(2, codeElements.size());
		ClassUnit baseClass = assertInstanceof(ClassUnit.class, codeElements.get(0));
		assertEquals("Base", baseClass.getName());
		ClassUnit subClass = assertInstanceof(ClassUnit.class, codeElements.get(1));
		assertEquals("SubClass", subClass.getName());
		// Base
		EList<CodeItem> baseElements = baseClass.getCodeElement();
		assertEquals(2, baseElements.size());
		// float f
		MemberUnit floatMember = assertInstanceof(MemberUnit.class, baseElements.get(1));
		assertEquals("f", floatMember.getName());
		assertInstanceof(FloatType.class, floatMember.getType());
		// InnerClass
		ClassUnit innerClass = assertInstanceof(ClassUnit.class, baseElements.get(0));
		assertEquals("InnerClass", innerClass.getName());
		assertEquals("children: " + innerClass.getCodeElement(), 1, innerClass.getCodeElement().size());
		// InnerClass.temp
		MemberUnit tempMember = assertInstanceof(MemberUnit.class, innerClass.getCodeElement().get(0));
		assertEquals("temp", tempMember.getName());
		assertInstanceof(FloatType.class, tempMember.getType());
		// SubClass
		assertEquals(1, subClass.getCodeElement().size());
		// SubClass.Show
		MethodUnit showMethod = assertInstanceof(MethodUnit.class, subClass.getCodeElement().get(0));
		assertEquals("Show", showMethod.getName());
		EList<AbstractCodeElement> showMethodElements = showMethod.getCodeElement();
		assertEquals(2, showMethodElements.size());
		// Signature
		Signature showSignature = assertInstanceof(Signature.class, showMethodElements.get(0));
		assertEquals("Show", showSignature.getName());
		// Method body
		BlockUnit showMethodBody = assertInstanceof(BlockUnit.class, showMethodElements.get(1));

		// SubClas.Extends
		EList<AbstractCodeRelationship> subClassRelations = subClass.getCodeRelation();
		assertEquals(1, subClassRelations.size());
		Extends extendsBase = assertInstanceof(Extends.class, subClassRelations.get(0));
		assertEquals(subClass, extendsBase.getFrom());
		assertEquals(baseClass, extendsBase.getTo());
		// TODO

		assertEquals(0, codeRelations.size());
	}

	@Test
	public void testReturnTypes() throws Exception {
		final String dirName = RESOURCE_TESTCASES_DIR + "22/";
		String filename = "NestedClass.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);

		EList<AbstractCodeElement> codeElements = compilationUnit.getCodeElement();
		EList<AbstractCodeRelationship> codeRelations = compilationUnit.getCodeRelation();
		// CompilationUnit
		assertEquals(1, codeElements.size());
		ClassUnit returnTypeClass = assertInstanceof(ClassUnit.class, codeElements.get(0));
		assertEquals("ReturnTypes", returnTypeClass.getName());

		EList<CodeItem> rtElements = returnTypeClass.getCodeElement();
		assertEquals(1, rtElements.size());

		TemplateUnit arrayMethodWrapper = assertInstanceof(TemplateUnit.class, rtElements.get(0));
		assertEquals(2, arrayMethodWrapper.getCodeElement().size());
		// TODO type parameter
		MethodUnit arrayMethod = assertInstanceof(MethodUnit.class, arrayMethodWrapper.getCodeElement().get(1));
		assertEquals("ToArrayOfArrays", arrayMethod.getName());
		EList<AbstractCodeElement> arrayMethodElements = arrayMethod.getCodeElement();
		// contains Signature, BlockUnit
		assertEquals(arrayMethodElements.toString(), 2, arrayMethodElements.size());
		// Signature
		Signature arrayMethodSignature = assertInstanceof(Signature.class, arrayMethodElements.get(0));
		EList<ParameterUnit> arrayMethodParams = arrayMethodSignature.getParameterUnit();
		assertEquals(2, arrayMethodParams.size());

		ParameterUnit returnTypeParam = assertInstanceof(ParameterUnit.class, arrayMethodParams.get(0));
		ArrayType firstArrayType = assertInstanceof(ArrayType.class, returnTypeParam.getType());
		assertEquals("T[][]", firstArrayType.getName());
		ArrayType firstBaseType = assertInstanceof(ArrayType.class, firstArrayType.getItemUnit().getType());
		assertEquals("T[]", firstBaseType.getName());
		Datatype secondBaseType = assertInstanceof(Datatype.class, firstBaseType.getItemUnit().getType());
		assertEquals("T", secondBaseType.getName());

		ParameterUnit param = assertInstanceof(ParameterUnit.class, arrayMethodParams.get(1));
		StringType paramType = assertInstanceof(StringType.class, param.getType());
		assertEquals("String", paramType.getName());
	}

	@Test
	public void testListAccess() throws Exception {
		final String dirName = RESOURCE_TESTCASES_DIR + "23/";
		String filename = "ListTest.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);

		EList<AbstractCodeElement> codeElements = compilationUnit.getCodeElement();
		EList<AbstractCodeRelationship> codeRelations = compilationUnit.getCodeRelation();
		// CompilationUnit
		assertEquals(1, codeElements.size());
		ClassUnit listTestClass = assertInstanceof(ClassUnit.class, codeElements.get(0));
		assertEquals("ListTest", listTestClass.getName());

		EList<CodeItem> listTestElements = listTestClass.getCodeElement();
		assertEquals(1, listTestElements.size());

		TemplateUnit getPropertyMethodWrapper = assertInstanceof(TemplateUnit.class, listTestElements.get(0));
		assertEquals(2, getPropertyMethodWrapper.getCodeElement().size());
		// TODO type parameter
		MethodUnit getPropertyMethod = assertInstanceof(MethodUnit.class, getPropertyMethodWrapper.getCodeElement()
				.get(1));
		assertEquals("GetProperty", getPropertyMethod.getName());
		EList<AbstractCodeElement> methodElements = getPropertyMethod.getCodeElement();
		// contains Signature, BlockUnit
		assertEquals(methodElements.toString(), 2, methodElements.size());
		// Signature
		Signature methodSignature = assertInstanceof(Signature.class, methodElements.get(0));
		EList<ParameterUnit> methodParams = methodSignature.getParameterUnit();
		assertEquals(4, methodParams.size());
		// List<String> strings
		ParameterUnit stringsParam = assertInstanceof(ParameterUnit.class, methodParams.get(2));
		ClassUnit listType = assertInstanceof(ClassUnit.class, stringsParam.getType());
		assertEquals("List", listType.getName());

		BlockUnit methodBody = assertInstanceof(BlockUnit.class, methodElements.get(1));
		assertEquals(1, methodBody.getCodeElement().size());

		assertInstanceof(ActionElement.class, methodBody.getCodeElement().get(0));
	}

	@Test
	public void testInterfaceDeclarations() throws Exception {
		final String dirName = RESOURCE_TESTCASES_DIR;
		String filename = "Interface.cs";

		CommonTree ast = Parsing.loadASTFromFile(dirName + filename);
		CompilationUnit compilationUnit = tranform(ast);

		assertEquals(1, compilationUnit.getCodeElement().size());
		// 1. type
		InterfaceUnit unit = assertInstanceof(InterfaceUnit.class, compilationUnit.getCodeElement().get(0));
		assertEquals("IEagle", unit.getName());

		assertEquals(unit.getCodeElement().toString(), 4, unit.getCodeElement().size());
		// 1. member
		MethodUnit method = assertInstanceof(MethodUnit.class, unit.getCodeElement().get(0));
		assertEquals("fly", method.getName());
		assertEquals(MethodKind.METHOD, method.getKind());
		assertEquals(ExportKind.PUBLIC, method.getExport());
	}

	private static <S, T extends S> void assertInstancesOf(final List<Class<T>> expected, final List<S> actual) {
		assertEquals(expected.size(), actual.size());
		for (int i = 0; i < actual.size(); i++) {
			assertInstanceof(expected.get(i), actual.get(i));
		}
	}

	private List<Class<? extends Datatype>> datatypesAsList(final Class<? extends Datatype>... classes) {
		return Arrays.asList(classes);
	}

	private CompilationUnit tranform(final CommonTree ast) {
		SourceFile sf = SourceFactory.eINSTANCE.createSourceFile();
		// 1. transformation
		preTransformator.transform(ast, sf);
		internalCodeModel.getCodeElement().addAll(preTransformator.getMappingResult());
		// 2. transformation
		memberDeclarationsTransformator.transform(ast, sf);
		// return memberDeclarationsTransformator.getMappingResult().get(0);
		// 3. transformation
		transformator.transform(ast, sf);
		return transformator.getMappingResult().get(0);
	}
}
