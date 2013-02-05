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

import mapping.KDMElementFactory;
import mapping.KDMElementFactory.GlobalKind;

import org.eclipse.core.runtime.NullProgressMonitor;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Attribute;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Segment;

import util.FileAccess;

public class SaveNamespaceTest {

	public static void main(final String[] args) throws IOException {
		Attribute attr = KDMElementFactory.createFullyQualifiedNameAttribute("TestNamespace");
		Namespace namespace = KDMElementFactory.createNamespaceUnit(attr);
		namespace.setName("TestNamespace");

		CodeModel internalCodeModel = KDMElementFactory
				.createGenericCodeModel("Internal CodeModel", GlobalKind.INTERNAL);
		AbstractCodeElement module = internalCodeModel.getCodeElement().get(0);

		Module nsModule = (Module) module;
		// add to namespace module
		nsModule.getCodeElement().add(namespace);
		// check whether one can go up to the module
		org.junit.Assert.assertEquals(nsModule, namespace.eContainer());
		// also add into namespace hierarchy
		((Namespace) (nsModule.getCodeElement().get(0))).getGroupedCode().add(namespace);

		Segment segment = KDMElementFactory.createSegment();
		segment.getModel().add(internalCodeModel);

		FileAccess.saveEcoreToXMI(segment, "testNamespace.xmi", new NullProgressMonitor());
	}
}
