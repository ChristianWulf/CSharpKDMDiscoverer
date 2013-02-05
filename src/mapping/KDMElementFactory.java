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

import java.util.Arrays;
import java.util.List;

import lang.csharp.CSharp4AST;
import mapping.action.MicroKDM;
import mapping.code.MoDiscoKDM;

import org.eclipse.core.runtime.Assert;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionElement;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionFactory;
import org.eclipse.gmt.modisco.omg.kdm.action.BlockUnit;
import org.eclipse.gmt.modisco.omg.kdm.action.Calls;
import org.eclipse.gmt.modisco.omg.kdm.action.Creates;
import org.eclipse.gmt.modisco.omg.kdm.action.Reads;
import org.eclipse.gmt.modisco.omg.kdm.action.Writes;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeFactory;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.ControlElement;
import org.eclipse.gmt.modisco.omg.kdm.code.DataElement;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.EnumeratedType;
import org.eclipse.gmt.modisco.omg.kdm.code.HasValue;
import org.eclipse.gmt.modisco.omg.kdm.code.Imports;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MemberUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.code.ParameterKind;
import org.eclipse.gmt.modisco.omg.kdm.code.ParameterUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Signature;
import org.eclipse.gmt.modisco.omg.kdm.code.StorableKind;
import org.eclipse.gmt.modisco.omg.kdm.code.StorableUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateParameter;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Value;
import org.eclipse.gmt.modisco.omg.kdm.core.Element;
import org.eclipse.gmt.modisco.omg.kdm.core.KDMEntity;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Annotation;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Attribute;
import org.eclipse.gmt.modisco.omg.kdm.kdm.KdmFactory;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Segment;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Stereotype;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFactory;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceRef;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceRegion;

import util.ListUtil;

public final class KDMElementFactory {

	public static enum GlobalKind {
		INTERNAL, EXTERNAL
	}

	private static final KdmFactory		KDM_FACTORY					= KdmFactory.eINSTANCE;
	public static final String			NAMESPACES_MODULE			= "Namespaces";
	public static final String			GLOBAL_NAMESPACE_NAME		= "global";
	private static final ActionFactory	ACTION_FACTORY				= ActionFactory.eINSTANCE;
	private static final CodeFactory	CODE_FACTORY				= CodeFactory.eINSTANCE;
	public static final String			FULLY_QUALIFIED_NAME_TAG	= "FullyQualifiedName";
	public static final String			STRUCT_ANNOTATION			= "represents a struct, not a class";
	public static final String			EVENT_DECLARATION			= "Event declaration";
	private static final String			DELEGATE_ANNOTATION			= "Delegate declaration";

	/**
	 * Types that are annotated with this string are added to the corresponding
	 * declaration as child. They are not added to a global type repository such
	 * as "external namespace".
	 */
	public static final String			LOCAL_TYPE					= "local Type";
	private static final String			VALUE_REPOSITORY_MODULE		= "ValueRepository";
	public static final String			TYPE_PARAMETER				= "type parameter";

	private KDMElementFactory() {
		// utility class
	}

	public static ClassUnit createClassUnit() {
		ClassUnit classUnit = CODE_FACTORY.createClassUnit();
		return classUnit;
	}

	public static MethodUnit createMethodUnit() {
		MethodUnit methodUnit = CODE_FACTORY.createMethodUnit();
		return methodUnit;
	}

	public static Signature createSignature() {
		Signature signature = CODE_FACTORY.createSignature();
		return signature;
	}

	public static ParameterUnit createParameterUnit(final String name) {
		ParameterUnit parameterUnit = CODE_FACTORY.createParameterUnit();
		parameterUnit.setName(name);
		return parameterUnit;
	}

	public static ParameterUnit createReturnType() {
		ParameterUnit parameterUnit = createParameterUnit("return type");
		parameterUnit.setKind(ParameterKind.RETURN);
		return parameterUnit;
	}

	public static Namespace createNamespaceUnit(final Attribute qualifiedNameAttribute) {
		Namespace namespaceUnit = CODE_FACTORY.createNamespace();
		namespaceUnit.getAttribute().add(qualifiedNameAttribute);
		return namespaceUnit;
	}

	public static Segment createSegment() {
		Segment segment = KDM_FACTORY.createSegment();
		return segment;
	}

	public static InterfaceUnit createInterfaceUnit() {
		InterfaceUnit interfaceUnit = CODE_FACTORY.createInterfaceUnit();
		return interfaceUnit;
	}

	public static ActionElement createMethodCall() {
		ActionElement actionElement = ACTION_FACTORY.createActionElement();
		actionElement.setKind(MicroKDM.getMethodCall());
		return actionElement;
	}

	public static Value createValue() {
		Value value = CODE_FACTORY.createValue();
		return value;
	}

	/**
	 * @param name
	 *            of the new CodeModel element
	 * @param kind
	 * @return a new CodeModel element
	 */
	public static CodeModel createGenericCodeModel(final String name, final GlobalKind kind) {
		// global namespaces have an empty qualified path
		Attribute qualifiedPathAttr = createFullyQualifiedNameAttribute("");

		Namespace globalNamespace = KDMElementFactory.createGlobalNamespace(kind);
		globalNamespace.getAttribute().add(qualifiedPathAttr);
		/*
		 * TODO stereotypes must be owned by ExtensionFamily who in turn must be
		 * owned by a Segment or CodeModel. Difficult for several CodeModels and
		 * Threads.
		 */
		Stereotype stereotype = KDM_FACTORY.createStereotype();
		stereotype.setName("global namespace container");
		// stereotype.setType(value);

		// TODO specific module
		/*
		 * do not detect module by name 'global' etc. A CompilationUnit (extends
		 * Module) could be named 'global' as well, for example, due to its
		 * filename.
		 */
		Module module = CODE_FACTORY.createModule();
		module.setName(NAMESPACES_MODULE);
		// module.getStereotype().add(stereotype);
		module.getCodeElement().add(globalNamespace);

		CodeModel codeModel = CODE_FACTORY.createCodeModel();
		codeModel.setName(name);
		codeModel.getCodeElement().add(module);

		return codeModel;
	}

	private static Namespace createGlobalNamespace(final GlobalKind kind) {
		Namespace namespace = CODE_FACTORY.createNamespace();
		namespace.setName(GLOBAL_NAMESPACE_NAME);
		addAnnotation(kind.toString(), namespace);
		return namespace;
	}

	public static Attribute createFullyQualifiedNameAttribute(final String value) {
		Attribute attr = KDM_FACTORY.createAttribute();
		attr.setTag(FULLY_QUALIFIED_NAME_TAG);
		attr.setValue(value);
		return attr;
	}

	public static Attribute getQualifiedNameAttribute(final Element kdmElement) {
		for (Attribute a : kdmElement.getAttribute()) {
			if (KDMElementFactory.FULLY_QUALIFIED_NAME_TAG.equals(a.getTag())) {
				return a;
			}
		}
		throw new IllegalStateException();
	}

	public static StorableUnit createStaticVariable() {
		StorableUnit storableUnit = CODE_FACTORY.createStorableUnit();
		storableUnit.setKind(StorableKind.STATIC);
		return storableUnit;
	}

	public static StorableUnit createLocalVariable() {
		StorableUnit storableUnit = CODE_FACTORY.createStorableUnit();
		storableUnit.setKind(StorableKind.LOCAL);
		return storableUnit;
	}

	public static StorableUnit createConstantVariable() {
		StorableUnit storableUnit = CODE_FACTORY.createStorableUnit();
		MoDiscoKDM.addModifierTo(storableUnit, "const static");
		storableUnit.setKind(StorableKind.STATIC);
		return storableUnit;
	}

	public static ActionElement createReadAccess(final ActionElement statement, final DataElement itemToBeRead) {
		Assert.isNotNull(itemToBeRead, "itemToBeRead");
		Reads readAccess = ACTION_FACTORY.createReads();
		readAccess.setFrom(statement);
		readAccess.setTo(itemToBeRead);

		ActionElement varAccess = MoDiscoKDM.createVariableAccess();
		varAccess.getActionRelation().add(readAccess);

		return varAccess;
	}

	public static ActionElement createWriteAccess(final ActionElement statement, final DataElement itemToBeWritten) {
		Assert.isNotNull(itemToBeWritten, "itemToBeWritten");
		Writes writeAccess = ACTION_FACTORY.createWrites();
		writeAccess.setFrom(statement);
		writeAccess.setTo(itemToBeWritten);

		ActionElement varAccess = MoDiscoKDM.createVariableAccess();
		varAccess.getActionRelation().add(writeAccess);

		return varAccess;
	}

	public static HasValue createHasValue(final DataElement left, final AbstractCodeElement right) {
		HasValue hasValue = CODE_FACTORY.createHasValue();
		hasValue.setFrom(left);
		hasValue.setTo(right);

		return hasValue;
	}

	public static Module createModule() {
		Module module = CODE_FACTORY.createModule();
		return module;
	}

	public static Module createValueRepository(final CodeModel codeModel) {
		Module valueRepo = createModule();
		valueRepo.setName(VALUE_REPOSITORY_MODULE);
		codeModel.getCodeElement().add(valueRepo);
		return valueRepo;
	}

	public static Calls createCalls(final ActionElement expression, final ControlElement method) {
		Calls calls = ACTION_FACTORY.createCalls();
		calls.setFrom(expression);
		calls.setTo(method);
		return calls;
	}

	public static MemberUnit createMemberUnit() {
		MemberUnit memberUnit = CODE_FACTORY.createMemberUnit();
		return memberUnit;
	}

	public static void addQualifiedAttributeTo(final Datatype datatype, final List<String> namespacePath) {
		String qualifiedName = ListUtil.combine(namespacePath, ".") + "." + datatype.getName();
		Attribute qualifiedNameAttribute = KDMElementFactory.createFullyQualifiedNameAttribute(qualifiedName);
		datatype.getAttribute().add(qualifiedNameAttribute);
	}

	public static BlockUnit createBlockUnit() {
		BlockUnit blockUnit = ACTION_FACTORY.createBlockUnit();
		return blockUnit;
	}

	public static Imports createImports(final CodeItem parent, final CodeItem namespaceOrType) {
		Imports imports = CODE_FACTORY.createImports();
		imports.setFrom(parent);
		imports.setTo(namespaceOrType);
		return imports;
	}

	public static Datatype createDatatype(final String name) {
		Datatype datatype = CODE_FACTORY.createDatatype();
		datatype.setName(name);
		return datatype;
	}

	public static Creates createCreates(final ActionElement expression, final Datatype datatype) {
		Creates creates = ACTION_FACTORY.createCreates();
		creates.setFrom(expression);
		creates.setTo(datatype);
		return creates;
	}

	public static Annotation createPropertyAnnotation() {
		Annotation anno = KDM_FACTORY.createAnnotation();
		anno.setText("Property declaration");
		return anno;
	}

	public static TemplateParameter createTypeParameter(final String name) {
		TemplateParameter typeParam = CODE_FACTORY.createTemplateParameter();
		typeParam.setName(name);
		// constraints on type parameters are not yet supported
		return typeParam;
	}

	public static boolean isTypeParameter(final ParameterUnit p) {
		return TYPE_PARAMETER.equals(p.getExt());
	}

	public static TemplateUnit createTemplateUnit(final String name) {
		TemplateUnit templateUnit = CODE_FACTORY.createTemplateUnit();
		templateUnit.setName(name);
		return templateUnit;
	}

	public static ActionElement createActionElementFallback() {
		ActionElement actionAlement = ACTION_FACTORY.createActionElement();
		actionAlement.setName("UNSUPPORTED expression");
		return actionAlement;
	}

	public static ClassUnit createStruct() {
		ClassUnit struct = CODE_FACTORY.createClassUnit();
		addAnnotation(STRUCT_ANNOTATION, struct);
		return struct;
	}

	public static DataElement createDummyDataElement(final String text) {
		Value value = KDMElementFactory.createValue();
		value.setName("dummy for " + text);
		return value;
	}

	public static CompilationUnit createCompilationUnit(final SourceFile sourceFile) {
		SourceRegion sourceRegion = SourceFactory.eINSTANCE.createSourceRegion();
		sourceRegion.setFile(sourceFile);
		sourceRegion.setPath(sourceFile.getPath());

		SourceRef sourceRef = SourceFactory.eINSTANCE.createSourceRef();
		sourceRef.getRegion().add(sourceRegion);

		CompilationUnit compilationUnit = CODE_FACTORY.createCompilationUnit();
		compilationUnit.setName(sourceFile.getName());
		compilationUnit.getSource().add(sourceRef);
		return compilationUnit;
	}

	public static DataElement createEventUnit() {
		// we use here a new DataElement for an event.
		// we may not reuse a MemberUnit because it is constrained to ClassUnits, however events can
		// be declared in InterfaceUnit, too.
		DataElement event = CODE_FACTORY.createDataElement();
		event.setName("EventUnit");
		event.setExt("event");

		addAnnotation(EVENT_DECLARATION, event);

		return event;
	}

	private static void addAnnotation(final String text, final KDMEntity kdmEntity) {
		Annotation anno = KDM_FACTORY.createAnnotation();
		anno.setText(text);
		kdmEntity.getAnnotation().add(anno);
	}

	public static DataElement createPropertyUnit(final boolean isStatic) {
		DataElement propertyDecl;
		if (isStatic) {
			propertyDecl = KDMElementFactory.createStaticVariable();
		} else {
			propertyDecl = KDMElementFactory.createMemberUnit();
		}

		// add annotation with the information that this element represents a
		// property
		propertyDecl.getAnnotation().add(KDMElementFactory.createPropertyAnnotation());
		return propertyDecl;
	}

	public static DataElement createMemberVariable(final int parentType, final boolean isStatic) {
		DataElement variable;
		if (parentType == CSharp4AST.EVENT_VARS_DECL) {
			variable = KDMElementFactory.createEventUnit();
			if (isStatic) {
				MoDiscoKDM.setModifiersTo(Arrays.asList("static"), variable);
			}
		} else if (isStatic) {
			variable = KDMElementFactory.createStaticVariable();
		} else {
			variable = KDMElementFactory.createMemberUnit();
		}
		return variable;
	}

	public static DataElement createIndexerUnit() {
		// we use here a new DataElement for an event.
		// we may not reuse a MemberUnit because it is constrained to ClassUnits, however indexer
		// can
		// be declared in InterfaceUnit, too.
		DataElement event = CODE_FACTORY.createDataElement();
		event.setName("IndexerUnit");
		event.setExt("this[..]");

		addAnnotation("Indexer declaration", event);

		return event;
	}

	public static EnumeratedType createEnum() {
		EnumeratedType enumeratedType = CODE_FACTORY.createEnumeratedType();
		return enumeratedType;
	}

	public static Datatype createDelegate() {
		Datatype delegateType = CODE_FACTORY.createDatatype();
		addAnnotation(DELEGATE_ANNOTATION, delegateType);
		return delegateType;
	}

	public static boolean isDelegateType(final KDMEntity kdmEntity) {
		for (Annotation anno : kdmEntity.getAnnotation()) {
			if (DELEGATE_ANNOTATION.equals(anno.getText())) return true;
		}
		return false;
	}

	public static boolean isStruct(final Element kdmElement) {
		for (Annotation anno : kdmElement.getAnnotation()) {
			if (STRUCT_ANNOTATION.equals(anno.getText())) return true;
		}
		return false;
	}

	public static TemplateParameter createTemplateParameter() {
		TemplateParameter templateParameter = CODE_FACTORY.createTemplateParameter();
		return templateParameter;
	}
}
