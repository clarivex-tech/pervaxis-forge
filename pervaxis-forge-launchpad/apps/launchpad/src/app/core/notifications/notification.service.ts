/**
 ************************************************************************
 * Copyright (C) 2026 Clarivex Technologies Private Limited
 * All Rights Reserved.
 *
 * NOTICE: All intellectual and technical concepts contained
 * herein are proprietary to Clarivex Technologies Private Limited
 * and may be covered by Indian and Foreign Patents,
 * patents in process, and are protected by trade secret or
 * copyright law. Dissemination of this information or reproduction
 * of this material is strictly forbidden unless prior written
 * permission is obtained from Clarivex Technologies Private Limited.
 *
 * Product:   Pervaxis Platform
 * Website:   https://clarivex.tech
 ************************************************************************
 */

import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
	providedIn: 'root',
})
export class NotificationService {
	private readonly snackBar = inject(MatSnackBar);

	showInfo(message: string): void {
		this.open(message, 'info-toast');
	}

	showSuccess(message: string): void {
		this.open(message, 'success-toast');
	}

	showError(message: string): void {
		this.open(message, 'error-toast', 7000);
	}

	private open(message: string, panelClass: string, duration = 5000): void {
		this.snackBar.open(message, 'Dismiss', {
			duration,
			horizontalPosition: 'right',
			verticalPosition: 'top',
			panelClass: [panelClass],
		});
	}
}
