package org.eclipse.modisco.csharp.discoverer;

import org.eclipse.core.resources.IFolder;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.emf.common.util.URI;
import org.eclipse.emf.ecore.resource.Resource;
import org.eclipse.modisco.infra.discovery.core.AbstractModelDiscoverer;
import org.eclipse.modisco.infra.discovery.core.exception.DiscoveryException;

public class CSharpFolderDiscoverer extends AbstractModelDiscoverer<IFolder> {

	public static final String ID = "org.eclipse.modisco.csharp.discoverer.folder"; //$NON-NLS-1$

	@Override
	public boolean isApplicableTo(final IFolder source) {
		// TODO if folder contains files with extension .cs
		return true;
	}

	@Override
	protected void basicDiscoverElement(final IFolder source, final IProgressMonitor monitor)
			throws DiscoveryException {
		// TODO
		URI targetURI = null;

		setDefaultTargetURI(targetURI);

		// start transformation
		CSharpTransformation transformation = new CSharpTransformation();
		Resource kdmResource = transformation.transformFolder(source, monitor);

		getResourceSet().getResources().add(kdmResource);
		setTargetModel(kdmResource);
	}

}
