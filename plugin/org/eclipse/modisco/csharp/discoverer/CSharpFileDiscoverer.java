package org.eclipse.modisco.csharp.discoverer;

import org.eclipse.core.resources.IFile;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.emf.common.util.URI;
import org.eclipse.emf.ecore.resource.Resource;
import org.eclipse.modisco.infra.discovery.core.AbstractModelDiscoverer;
import org.eclipse.modisco.infra.discovery.core.exception.DiscoveryException;

public class CSharpFileDiscoverer extends AbstractModelDiscoverer<IFile> {

	public static final String	ID	= "org.eclipse.modisco.csharp.discoverer.file"; //$NON-NLS-1$

	@Override
	public boolean isApplicableTo(final IFile source) {
		return source.exists() && isCSharpFile(source);
	}

	// TODO add more sophisticated detection logic
	private static boolean isCSharpFile(final IFile source) {
		return source.getFileExtension().equals("cs"); //$NON-NLS-1$
	}

	@Override
	protected void basicDiscoverElement(final IFile source, final IProgressMonitor monitor)
			throws DiscoveryException {
		// TODO
		URI targetURI = null;

		setDefaultTargetURI(targetURI);

		// start transformation
		CSharpTransformation transformation = new CSharpTransformation();
		Resource kdmResource = transformation.transformFile(source, monitor);

		getResourceSet().getResources().add(kdmResource);
		setTargetModel(kdmResource);
	}

}
