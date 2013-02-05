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

package mapping.code.namespace;

import static org.junit.Assert.assertEquals;

import java.util.Arrays;

import mapping.KDMElementFactory;
import mapping.KDMElementFactory.GlobalKind;
import mapping.code.resolver.NamespaceResolver;
import mapping.code.resolver.Resolver;

import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Attribute;
import org.junit.Before;
import org.junit.Test;

public class TestNamespaceStackTest {

	private NamespaceStack	namespaceStack;
	private Namespace		globalNamespace;

	@Before
	public void createNamespaceStack() throws Exception {
		CodeModel codeModel = KDMElementFactory.createGenericCodeModel("test", GlobalKind.INTERNAL);
		globalNamespace = NamespaceSearcher.getGlobalNamespaceFrom(codeModel);
		NamespaceSearcher searcher = null;
		Resolver resolver = new NamespaceResolver();
		namespaceStack = new NamespaceStack(globalNamespace, searcher, resolver);
	}

	@Test
	public void testPush() {
		Attribute attr = KDMElementFactory.createFullyQualifiedNameAttribute("");
		Namespace namespace = KDMElementFactory.createNamespaceUnit(attr);
		namespace.setName("1");
		namespaceStack.pushNamespace(namespace);
		assertEquals(namespace, namespaceStack.getCurrentNamespace());
	}

	@Test
	public void testPushPush() {
		Attribute attr1 = KDMElementFactory.createFullyQualifiedNameAttribute("");
		Namespace namespace1 = KDMElementFactory.createNamespaceUnit(attr1);
		namespace1.setName("1");
		namespaceStack.pushNamespace(namespace1);
		Attribute attr2 = KDMElementFactory.createFullyQualifiedNameAttribute("");
		Namespace namespace2 = KDMElementFactory.createNamespaceUnit(attr2);
		namespace1.setName("2");
		namespaceStack.pushNamespace(namespace2);

		assertEquals(namespace2, namespaceStack.getCurrentNamespace());
		assertEquals(Arrays.asList(globalNamespace, namespace1, namespace2),
				namespaceStack.getCurrentNamespacePath());
	}

	@Test
	public void testPop() {
		Attribute attr = KDMElementFactory.createFullyQualifiedNameAttribute("");
		Namespace namespace = KDMElementFactory.createNamespaceUnit(attr);
		namespace.setName("1");
		namespaceStack.pushNamespace(namespace);
		assertEquals(namespace, namespaceStack.popCurrentNamespace(1));
		assertEquals(Arrays.asList(globalNamespace), namespaceStack.getCurrentNamespacePath());
	}

	@Test
	public void testPopPop() {
		Attribute attr1 = KDMElementFactory.createFullyQualifiedNameAttribute("");
		Namespace namespace1 = KDMElementFactory.createNamespaceUnit(attr1);
		namespace1.setName("1");
		namespaceStack.pushNamespace(namespace1);
		Attribute attr2 = KDMElementFactory.createFullyQualifiedNameAttribute("");
		Namespace namespace2 = KDMElementFactory.createNamespaceUnit(attr2);
		namespace1.setName("2");
		namespaceStack.pushNamespace(namespace2);

		assertEquals(namespace2, namespaceStack.popCurrentNamespace(1));
		assertEquals(Arrays.asList(globalNamespace, namespace1),
				namespaceStack.getCurrentNamespacePath());
	}

}
