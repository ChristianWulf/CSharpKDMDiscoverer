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

import mapping.KDMElementFactory;

import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeFactory;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;

/**
 * This test shows that the KDM Namespace element has no containment relationship to its classes.
 * @author Nogge
 *
 */
public class GoUp {

	/**
	 * @param args
	 */
	public static void main(final String[] args) {

		ClassUnit classUnit = CodeFactory.eINSTANCE.createClassUnit();

		CodeItem c= KDMElementFactory.createInterfaceUnit();
		System.out.println(c.eContainer());
		classUnit.getCodeElement().add(c);
		System.out.println(c.eContainer());
		System.out.println(c.eContainingFeature());
		System.out.println(c.eContainmentFeature());

		Namespace namespace = CodeFactory.eINSTANCE.createNamespace();
		namespace.getGroupedCode().add(classUnit);
		namespace.getGroupedElement().add(classUnit);

		//		Package package1 = CodeFactory.eINSTANCE.createPackage();
		//		package1.getCodeElement().add(classUnit);

		System.out.println(classUnit.eContainer());
		System.out.println(classUnit.eContainingFeature());
		System.out.println(classUnit.eContainmentFeature());
		System.out.println(classUnit.eContents());
		System.out.println(classUnit.getGroup());

		//		System.out.println(package1.getGroupedElement());
	}

}
