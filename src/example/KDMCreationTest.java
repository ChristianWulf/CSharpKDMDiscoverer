/***************************************************************************
 * Copyright 2012 by
 * + Christian-Albrechts-University of Kiel
 * + Department of Computer Science
 * + Software Engineering Group
 * and others.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ***************************************************************************/

package example;

import java.io.IOException;

import org.eclipse.core.runtime.NullProgressMonitor;
import org.eclipse.emf.common.util.EList;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionFactory;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionPackage;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeRelationship;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeFactory;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CodePackage;
import org.eclipse.gmt.modisco.omg.kdm.code.Extends;
import org.eclipse.gmt.modisco.omg.kdm.code.Package;
import org.eclipse.gmt.modisco.omg.kdm.core.CoreFactory;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Attribute;
import org.eclipse.gmt.modisco.omg.kdm.kdm.KdmFactory;
import org.eclipse.gmt.modisco.omg.kdm.kdm.KdmPackage;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Segment;
import org.eclipse.gmt.modisco.omg.kdm.kdm.TagDefinition;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFactory;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.FileAccess;

public class KDMCreationTest {

	public static void main(final String[] args) throws IOException {
		ClassUnit classUnit = createClassUnit();
		FileAccess.saveEcoreToXMI(classUnit, "ecore/" + classUnit.getName() + ".xmi", new NullProgressMonitor());

		Package package1 = createPackageUnit();
		package1.getCodeElement().add(classUnit);
		FileAccess.saveEcoreToXMI(package1, "ecore/TestPackage.xmi", new NullProgressMonitor());

		CodeModel codeModel = createCodeModel();
		codeModel.getCodeElement().add(package1);
		FileAccess.saveEcoreToXMI(codeModel, "ecore/TestCodeModel.xmi", new NullProgressMonitor());

		Segment segment = createSegment();
		segment.getModel().add(codeModel);
		FileAccess.saveEcoreToXMI(segment, "ecore/TestSegment.xmi", new NullProgressMonitor());

		ActionFactory actionFactory = ActionPackage.eINSTANCE.getActionFactory();
		actionFactory.createActionElement();
	}

	private static Package createPackageUnit() {
		CodeFactory codeFactory = CodePackage.eINSTANCE.getCodeFactory();

		Package createPackage = codeFactory.createPackage();
		createPackage.setName("Test package");

		return createPackage;
	}

	private static Segment createSegment() {
		KdmFactory kdmFactory = KdmPackage.eINSTANCE.getKdmFactory();

		Segment createSegment = kdmFactory.createSegment();
		createSegment.setName("Test segment");
		createSegment.getModel();
		createSegment.getSegment();

		return createSegment;
	}

	private static CodeModel createCodeModel() {
		CodeFactory codeFactory = CodePackage.eINSTANCE.getCodeFactory();

		CodeModel codeModel = codeFactory.createCodeModel();
		codeModel.setName("Test code model");

		codeModel.getAnnotation();
		codeModel.getAttribute();
		codeModel.getAudit();
		codeModel.getCodeElement();
		codeModel.getExtension();
		codeModel.getStereotype();
		codeModel.getTaggedValue();

		return codeModel;
	}

	private static ClassUnit createClassUnit() {
		CodeFactory codeFactory = CodePackage.eINSTANCE.getCodeFactory();

		ClassUnit classUnit = codeFactory.createClassUnit();
		classUnit.setName("TestClass");
		classUnit.setIsAbstract(Boolean.FALSE);

		EList<Attribute> attributes = classUnit.getAttribute();
		addAttribute(attributes, "name");

		classUnit.getSource();

		EList<CodeItem> codeElement = classUnit.getCodeElement();
		// check with instanceof: method declaration, namespace, type,
		// interfaceUnit
		EList<AbstractCodeRelationship> codeRelation = classUnit.getCodeRelation();
		// check with instanceof: extends, implements, include, hasType
		Extends createExtends = codeFactory.createExtends();
		//		createExtends.setFrom(value);
		//		createExtends.setTo(value);
		codeRelation.add(createExtends);

		return classUnit;
	}

	private static void addAttribute(final EList<Attribute> attributes, final String attrName) {
		Attribute createAttribute = KdmFactory.eINSTANCE.createAttribute();
		createAttribute.setValue("example name");
		createAttribute.setTag("TestClassTag");

		TagDefinition createTagDefinition = KdmFactory.eINSTANCE.createTagDefinition();
		createTagDefinition.setTag("TestClassTag");
		createTagDefinition.setType("TestClass");

		SourceFile createSourceFile = SourceFactory.eINSTANCE.createSourceFile();

		CoreFactory.eINSTANCE.createAggregatedRelationship();

		attributes.add(createAttribute);
	}
}
