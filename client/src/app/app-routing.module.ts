import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MembersListComponent } from './members/members-list/members-list.component';
import { MembersDetailsComponent } from './members/members-details/members-details.component';
import { ListsComponent } from './lists/lists.component';
import { MessagesComponent } from './messages/messages.component';
import { AuthGuard } from './_guard/auth.guard';
import { TestErrorComponent } from './errors/test-error/test-error.component';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { PreventUnsavedChangesGuard } from './_guards/prevent-unsaved-changes.guard';
import { MemberDetailedResolver } from './_resolvers/member-detailed.resolver';
import { AdminPannelComponent } from './admin/admin-pannel/admin-pannel.component';
import { AdminGuard } from './_guards/admin.guard';

const routes: Routes = [
  { path: '', component: HomeComponent },
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      { path: 'members', component: MembersListComponent },
      {
        path: 'members/:username',
        component: MembersDetailsComponent,
        resolve: { member: MemberDetailedResolver },
      },
      {
        path: 'member/edit',
        component: MemberEditComponent,
        canDeactivate: [PreventUnsavedChangesGuard],
      },
      { path: 'lists', component: ListsComponent },
      { path: 'messages', component: MessagesComponent },
      { path: 'errors', component: TestErrorComponent },
      { path: 'not-found', component: NotFoundComponent },
      { path: 'server-error', component: ServerErrorComponent },
      {
        path: 'admin',
        component: AdminPannelComponent,
        canActivate: [AdminGuard],
      },
    ],
  },

  { path: '**', component: NotFoundComponent, pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
