package org.eclipse.modisco.csharp.discoverer.internal;

import java.io.IOException;

import org.eclipse.core.resources.IFile;
import org.eclipse.core.resources.IWorkspace;
import org.eclipse.core.resources.ResourcesPlugin;
import org.eclipse.core.runtime.IPath;
import org.eclipse.core.runtime.NullProgressMonitor;
import org.eclipse.core.runtime.Path;
import org.eclipse.emf.ecore.resource.Resource;
import org.eclipse.modisco.csharp.discoverer.CSharpFileDiscoverer;
import org.eclipse.modisco.infra.discovery.core.AbstractModelDiscoverer;
import org.eclipse.modisco.infra.discovery.core.IDiscoverer;
import org.eclipse.modisco.infra.discovery.core.IDiscoveryManager;
import org.eclipse.modisco.infra.discovery.core.exception.DiscoveryException;

import util.FileAccess;

public final class Test {

	private Test() {
		// utility class
	}

	/**
	 * @param args
	 * @throws DiscoveryException
	 * @throws IOException
	 */
	public static void main(final String[] args) throws DiscoveryException, IOException {
		// Collection<DiscovererDescription> discoverers =
		// IDiscoveryManager.INSTANCE.getDiscoverers();
		// System.out.println(discoverers);
		@SuppressWarnings("unchecked")
		IDiscoverer<IFile> discoverer = (IDiscoverer<IFile>) IDiscoveryManager.INSTANCE
		.createDiscovererImpl(CSharpFileDiscoverer.ID);

		IWorkspace workspace = ResourcesPlugin.getWorkspace();
		IPath path = Path.fromOSString("csharp-examples/HelloWorld.cs");
		IFile file = workspace.getRoot().getFile(path);

		discoverer.discoverElement(file, new NullProgressMonitor());

		Resource targetModel = ((AbstractModelDiscoverer<IFile>) discoverer)
				.getTargetModel();
		FileAccess.saveEcoreToXMI(targetModel.getContents().get(0),
				"csharp-examples/Test.xmi", new NullProgressMonitor());
	}

}
