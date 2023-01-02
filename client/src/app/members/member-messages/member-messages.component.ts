import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Message } from 'src/app/_models/Message';
import { MessagesService } from '../../_services/messages.service';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm?: NgForm;
  @Input() username?: string;
  @Input() messages: Message[] = [];
  messageContent = '';
  constructor(private messagesService: MessagesService) {}

  ngOnInit(): void {}

  sendMessage() {
    if (!this.username) return;

    this.messagesService
      .sendMessage(this.username, this.messageContent)
      .subscribe({
        next: (message) => {
          this.messages.push(message);
          this.messageForm?.reset();
        },
      });
  }
}
