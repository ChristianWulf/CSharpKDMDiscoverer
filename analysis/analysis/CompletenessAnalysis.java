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

package analysis;

import java.io.File;
import java.io.FileNotFoundException;
import java.util.Collection;
import java.util.Collections;
import java.util.LinkedList;
import java.util.List;

import mapping.KDMElementFactory;
import mapping.code.namespace.NamespaceSearcher;
import mapping.source.InventoryModelCreator;
import mapping.source.InventoryModelWalker;
import mapping.source.visitor.SourceFileNamesVisitor;
import mapping.util.KDMChildHelper;
import mapping.util.ModelCreationHelper;
import mapping.util.NameComparator;

import org.eclipse.core.runtime.NullProgressMonitor;
import org.eclipse.emf.common.util.TreeIterator;
import org.eclipse.emf.ecore.EObject;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.EnumeratedType;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MemberUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.code.StorableUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateUnit;
import org.eclipse.gmt.modisco.omg.kdm.core.KDMEntity;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Segment;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryModel;
import org.junit.Assert;

import util.Logger;
import util.MyProperties;
import example.ProgressMonitorExample;

public class CompletenessAnalysis {

	private static final Logger			LOG				= new Logger(CompletenessAnalysis.class);
	private final InventoryModel		inventoryModel;
	private final CodeModel				internalCodeModel;
	private final CodeModel				externalCodeModel;
	private final List<CompilationUnit>	compilationUnits;

	private final List<CompilationUnit>	compilations	= new LinkedList<CompilationUnit>();
	private final List<KDMEntity>		namespaces		= new LinkedList<KDMEntity>();
	private final List<KDMEntity>		classes			= new LinkedList<KDMEntity>();
	private final List<KDMEntity>		interfaces		= new LinkedList<KDMEntity>();
	private final List<KDMEntity>		structs			= new LinkedList<KDMEntity>();
	private final List<KDMEntity>		enums			= new LinkedList<KDMEntity>();
	private final List<KDMEntity>		delegates		= new LinkedList<KDMEntity>();
	private final List<KDMEntity>		members			= new LinkedList<KDMEntity>();
	private final List<KDMEntity>		methods			= new LinkedList<KDMEntity>();

	public CompletenessAnalysis(final Segment segment) {
		inventoryModel = (InventoryModel) segment.getModel().get(0);
		internalCodeModel = (CodeModel) segment.getModel().get(1);
		externalCodeModel = (CodeModel) segment.getModel().get(2);

		compilationUnits = KDMChildHelper.filterChildrenByType(internalCodeModel.getCodeElement(),
				CompilationUnit.class);
	}

	public static void main(final String[] args) throws FileNotFoundException {
		try {
			// assertNordicAnalytics();
			assertSharpDevelop();
		} catch (Exception e) {
			LOG.warning(e);
		}
	}

	private static void assertNordicAnalytics() throws FileNotFoundException {
		String dirname = "D:/SW-development/NordicAnalytics";
		if (!new File(dirname).exists()) throw new FileNotFoundException(dirname);

		InventoryModelCreator inventoryModelCreator = new InventoryModelCreator();
		InventoryModel inventoryModel = inventoryModelCreator.openDirectory(dirname, new NullProgressMonitor());

		InventoryModelWalker walker = new InventoryModelWalker(inventoryModel);

		ProgressMonitorExample monitor = new ProgressMonitorExample();
		Segment segment = ModelCreationHelper.buildKDMInstance(inventoryModel, monitor, new MyProperties(), 2);

		SourceFileNamesVisitor fileCounter = new SourceFileNamesVisitor("C#");
		walker.walk(fileCounter);

		CompletenessAnalysis analysis = new CompletenessAnalysis(segment);
		analysis.walk();

		// expected values are determined by the matured analysis application
		// "NDepend"
		// # Assemblies : 1
		// # Namespaces : 109
		// # Types : 1 477 (1355 w/o delegates)
		// # Methods : 11 929 (6799 w/o class constructors cctor)
		// # Fields : 7 169

		Assert.assertEquals(printList(analysis.compilations), fileCounter.getSourceFileNames().size(),
				analysis.compilations.size());

		// expected:<109> but was:<113>
		Assert.assertEquals(109 + 4, analysis.namespaces.size());

		Assert.assertEquals(7, analysis.structs.size());

		// expected:<1035> but was:<1036>
		// zu faul den einen �bersch�ssigen zu suchen
		Assert.assertEquals(1035 + 1, analysis.classes.size());

		// expected:<121> but was:<123>
		// ICorrelationBase
		// ICurve
		Assert.assertEquals(121 + 2, analysis.interfaces.size());

		// expected:<192> but was:<193>
		// zu faul den einen �bersch�ssigen zu suchen
		Assert.assertEquals(192 + 1, analysis.enums.size());

		// expected:<1355> but was:<1359>
		// 4 sind von oben bekannt
		Assert.assertEquals(1355 + 4, analysis.countTypes());

		// expected:<7169> but was:<8356>
		Assert.assertEquals(7169, analysis.members.size());

		// expected:<6799> but was:<6250>
		Assert.assertEquals(6799, analysis.methods.size());
	}

	private static String printList(final List<? extends KDMEntity> eobjects) {
		Collections.sort(eobjects, new NameComparator());

		StringBuilder builder = new StringBuilder(eobjects.size() * 15);
		for (KDMEntity entity : eobjects) {
			builder.append(entity.getName());
			builder.append("\n");
		}
		return builder.toString();
	}

	private static String printFullyQualifiedList(final List<KDMEntity> eobjects) {
		Collections.sort(eobjects, new NameComparator());

		StringBuilder builder = new StringBuilder(eobjects.size() * 15);
		for (KDMEntity entity : eobjects) {
			builder.append(entity.getAttribute().get(0).getValue());
			builder.append("\n");
		}
		return builder.toString();
	}

	/**
	 * @param prefix
	 * @param namespaces2
	 * @return
	 */
	private static List<KDMEntity> filter(final List<KDMEntity> eobjects, final String prefix) {
		List<KDMEntity> filteredList = new LinkedList<KDMEntity>();

		for (KDMEntity entity : eobjects) {
			String namespace = entity.getAttribute().get(0).getValue();
			if (namespace.startsWith(prefix)) {
				filteredList.add(entity);
			}
		}

		return filteredList;
	}

	private static void assertSharpDevelop() throws FileNotFoundException {
		String dirname = "D:/SUT/SharpDevelop_4.1.0.8000_Source/src";
		if (!new File(dirname).exists()) throw new FileNotFoundException(dirname);

		InventoryModelCreator inventoryModelCreator = new InventoryModelCreator();
		InventoryModel inventoryModel = inventoryModelCreator.openDirectory(dirname, new NullProgressMonitor());

		InventoryModelWalker walker = new InventoryModelWalker(inventoryModel);

		ProgressMonitorExample monitor = new ProgressMonitorExample();
		Segment segment = ModelCreationHelper.buildKDMInstance(inventoryModel, monitor, new MyProperties(), 1);

		SourceFileNamesVisitor fileCounter = new SourceFileNamesVisitor("C#");
		walker.walk(fileCounter);

		CompletenessAnalysis analysis = new CompletenessAnalysis(segment);
		analysis.walk();

		System.out.println("compilations: " + analysis.compilations.size());
		System.out.println(analysis.namespaces.size());
		System.out.println(analysis.structs.size());
		System.out.println(analysis.classes.size());
		System.out.println(analysis.interfaces.size());
		System.out.println(analysis.enums.size());
		System.out.println("all types: " + analysis.countTypes());
		System.out.println(analysis.members.size());
		System.out.println(analysis.methods.size());
		System.out.println("delegates: " + analysis.delegates.size());
		// compilations: 6403
		// 596
		// 120
		// 7462
		// 588
		// 355
		// 8525
		// 28494
		// 41691
		// Stand: 13.12.2012 (all phases)
		// compilations: 6718
		// 595
		// 120
		// 7774
		// 588
		// 355
		// 8837
		// 29606
		// 42461
		// Stand: 14.12.2012 (2 phases)

		// SELECT NAMESPACES WHERE NameLike "ICSharp"
		// count: 333
		List<KDMEntity> filteredList = filter(analysis.namespaces, "ICSharp");
		System.out.println("'ICSharp*' count: " + filteredList.size());
		// System.out.println(printFullyQualifiedList(filteredList));

		// System.out.println("structs:");
		// System.out.println(printFullyQualifiedList(analysis.structs));
		// System.out.println("classes:");
		// System.out.println(printFullyQualifiedList(analysis.classes));
		// System.out.println("interfaces:");
		// System.out.println(printFullyQualifiedList(analysis.interfaces));
		// System.out.println("enums:");
		// System.out.println(printFullyQualifiedList(analysis.enums));

		// expected values are determined by the matured analysis application "NDepend"
		// # Assemblies :
		// # Namespaces :
		// # Types : (XXX w/o delegates)
		// # Methods : (XXX w/o class constructors cctor)
		// # Fields :

		// expected:<6756> but was:<6741>, -15
		// printList(analysis.compilations),
		Collection<String> recognizedPaths;
		List<String> sourceFileNames;

		recognizedPaths = toStringPaths(analysis.compilations);
		sourceFileNames = new LinkedList<String>(fileCounter.getSourceFileNames());
		recognizedPaths.removeAll(sourceFileNames);
		// check whether each compilation unit corresponds to a C# file on the file system
		Assert.assertEquals(Collections.EMPTY_LIST, recognizedPaths);

		recognizedPaths = toStringPaths(analysis.compilations);
		sourceFileNames = new LinkedList<String>(fileCounter.getSourceFileNames());
		sourceFileNames.removeAll(recognizedPaths);
		// check whether each C# file on the file system corresponds to a compilation unit
		Assert.assertEquals(Collections.EMPTY_LIST, sourceFileNames);

		// expected:<xxx> but was:<596>
		// Assert.assertEquals(, analysis.namespaces.size());

		// Assert.assertEquals(7, analysis.structs.size());

		// expected:<XXX> but was:<XXX>
		// zu faul den einen überschüssigen zu suchen
		// Assert.assertEquals(XXX, analysis.classes.size());

		// expected:<XXX> but was:<XXX>
		// ICorrelationBase
		// ICurve
		// Assert.assertEquals(XXX, analysis.interfaces.size());

		// expected:<XXX> but was:<XXX>
		// zu faul den einen überschüssigen zu suchen
		// Assert.assertEquals(XXX, analysis.enums.size());

		// expected:<XXX> but was:<XXX>
		// 4 sind von oben bekannt
		// Assert.assertEquals(XXX, analysis.countTypes());

		// expected:<XXX> but was:<XXX>
		// Assert.assertEquals(XXX, analysis.members.size());

		// expected:<XXX> but was:<XXX>
		// Assert.assertEquals(XXX, analysis.methods.size());
	}

	/**
	 * @param compilations
	 * @return
	 */
	private static Collection<String> toStringPaths(final List<CompilationUnit> compilations) {
		Collection<String> paths = new LinkedList<String>();
		for (CompilationUnit c : compilations) {
			String path = c.getSource().get(0).getRegion().get(0).getFile().getPath();
			paths.add(path);
		}
		return paths;
	}

	private void walk() {
		TreeIterator<EObject> treeIterator = internalCodeModel.eAllContents();
		while (treeIterator.hasNext()) {
			EObject obj = treeIterator.next();

			if (!(obj instanceof KDMEntity)) continue;

			KDMEntity kdmEntity = (KDMEntity) obj;
			if (kdmEntity instanceof CompilationUnit) {
				compilations.add((CompilationUnit) kdmEntity);
			} else if (kdmEntity instanceof Namespace) {
				namespaces.add(kdmEntity);
			} else if (kdmEntity instanceof ClassUnit) {
				if (KDMElementFactory.isStruct(kdmEntity)) {
					structs.add(getMainUnit(kdmEntity));
				} else {
					classes.add(getMainUnit(kdmEntity));
				}
			} else if (kdmEntity instanceof InterfaceUnit) {
				interfaces.add(getMainUnit(kdmEntity));
			} else if (kdmEntity instanceof EnumeratedType) {
				enums.add(kdmEntity);
			} else if (kdmEntity instanceof StorableUnit) {
				if (kdmEntity.eContainer() instanceof ClassUnit) {
					members.add(kdmEntity);
				}
			} else if (kdmEntity instanceof MemberUnit) {
				members.add(kdmEntity);
			} else if (kdmEntity instanceof MethodUnit) {
				methods.add(kdmEntity);
			} else if (kdmEntity instanceof Datatype) {
				if (KDMElementFactory.isDelegateType(kdmEntity)) {
					delegates.add(kdmEntity);
				}
			}
		}
	}

	/**
	 * @param kdmEntity
	 * @return
	 */
	private KDMEntity getMainUnit(final KDMEntity kdmEntity) {
		if (kdmEntity.eContainer() instanceof TemplateUnit) System.out.println("template: " + kdmEntity.eContainer());
		return (KDMEntity) ((kdmEntity.eContainer() instanceof TemplateUnit) ? kdmEntity.eContainer() : kdmEntity);
	}

	private int countCompilationUnits() {
		return compilationUnits.size();
	}

	private int countNamespaces() {
		Namespace globalNs = NamespaceSearcher.getGlobalNamespaceFrom(internalCodeModel);
		Module nsModule = (Module) globalNs.eContainer();
		List<Namespace> namespaces = KDMChildHelper.filterChildrenByType(nsModule.getCodeElement(), Namespace.class);
		int size = namespaces.size();
		return size - 1; // w/o global namespace
	}

	private int countClasses() {
		return count(ClassUnit.class);
	}

	private int countInterfaces() {
		return count(InterfaceUnit.class);
	}

	private int countEnums() {
		return count(EnumeratedType.class);
	}

	private int countStructs() {
		int count = 0;
		for (CompilationUnit cu : compilationUnits) {
			List<? extends AbstractCodeElement> elements = KDMChildHelper.filterChildrenByType(cu.getCodeElement(),
					ClassUnit.class);
			for (AbstractCodeElement ace : elements) {
				if (KDMElementFactory.isStruct(ace)) count++;
			}
		}
		return count;
	}

	private int countTypes() {
		return classes.size() + interfaces.size() + enums.size() + structs.size() + delegates.size();
	}

	private int count(final Class<? extends AbstractCodeElement> clazz) {
		List<AbstractCodeElement> aces = new LinkedList<AbstractCodeElement>();
		for (CompilationUnit cu : compilationUnits) {
			List<Datatype> types = findTypes(cu.getCodeElement());

			List<? extends AbstractCodeElement> elements = KDMChildHelper.filterChildrenByType(types, clazz);
			aces.addAll(elements);
		}

		// Collections.sort(aces, new NameComparator());
		// for (AbstractCodeElement ace : aces) {
		// System.out.println(ace.getName());
		// }

		return aces.size();
	}

	private List<Datatype> findTypes(final List<? extends AbstractCodeElement> elements) {
		List<Datatype> types = new LinkedList<Datatype>();
		for (AbstractCodeElement ace : elements) {
			if (ace instanceof ClassUnit) {
				types.add((ClassUnit) ace);
				types.addAll(findTypes(((ClassUnit) ace).getCodeElement()));
			} else if (ace instanceof InterfaceUnit) {
				types.add((InterfaceUnit) ace);
				types.addAll(findTypes(((InterfaceUnit) ace).getCodeElement()));
			} else if (ace instanceof EnumeratedType) {
				types.add((EnumeratedType) ace);
				types.addAll(findTypes(((EnumeratedType) ace).getCodeElement()));
			}
		}
		return types;
	}
}
