import { Component, Input, OnInit } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/Member';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() member: Member;
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl: string = environment.apiUrl;
  user: User;
  constructor(private accountServive: AccountService,
    private membersService: MembersService) {
    this.accountServive.currentUser$.pipe(take(1)).subscribe(user => this.user = user)
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: any){
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.user.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload:true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024 //10 MB
    });
    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if(response){
        const photo = JSON.parse(response);
        this.member.photos.push(photo);
      }
    }
  }

  setMainPhoto(photo){
    this.membersService.setMainPhoto(photo.id).subscribe(()=>{
      this.user.photoUrl = photo.url;
      this.accountServive.setCurrentUser(this.user);
      this.member.photoUrl = photo.url;
      this.member.photos.forEach(member => {
        if(member.isMain) member.isMain = false;
        if(member.id == photo.id)
        {
          member.isMain = true;
        }
      })

    })
  }

  deletePhoto(photoId: number){
    this.membersService.deletePhoto(photoId).subscribe(() => {
      this.member.photos = this.member.photos.filter(x => x.id != photoId);
    });
  }

}
