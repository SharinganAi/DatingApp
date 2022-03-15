import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { AccountsService } from '../_services/accounts.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
@Output() cancelRegister = new EventEmitter();
  model:
  {
    userName: string;
    password: string;
  }


  constructor(private accountService:AccountsService) { }

  ngOnInit(): void {
    this.model = { userName :"", password :""};
  }

  register(){
    this.accountService.register(this.model).subscribe(res =>{
      this.cancel();
    }, err =>{
      console.log(err);
    });
  }

  cancel(){
    this.cancelRegister.emit(false);
  }

}
