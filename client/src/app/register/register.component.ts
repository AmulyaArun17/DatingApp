import { Component,EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  model: any = {};
  @Output() cancelRegister = new EventEmitter<boolean>();
  constructor(private accountService: AccountService,
    private toastr: ToastrService) { }

  ngOnInit(): void {
  }

  register(){
    this.accountService.register(this.model).subscribe(res=>{
      console.log(res);
      this.cancel();
    },
    error=>{
      console.log(error);
      this.toastr.error(error.error);
    });
  }
  
  cancel(){
    console.log('cancelled');
    this.cancelRegister.emit(false);
  }
}
